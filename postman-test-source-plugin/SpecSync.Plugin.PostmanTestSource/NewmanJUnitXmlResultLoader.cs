using SpecSync.PublishTestResults.Loaders;
using SpecSync.PublishTestResults;
using SpecSync.Tracing;
using System.Text.RegularExpressions;

namespace SpecSync.Plugin.PostmanTestSource;

public class NewmanJUnitXmlResultLoader : JUnitXmlTestCaseAsStepResultLoaderBase
{
    public const string TestResultFileFormat = "NewmanJUnitXml";

    protected override string TestFrameworkIdentifier => NewmanJUnitXmlResultMatcher.TestFrameworkIdentifier;
    public override string ServiceDescription => $"{TestResultFileFormat}: Postman Newman JUnit XML result";

    public override bool CanProcess(TestResultLoaderProviderArgs args)
        => args.TestResultConfiguration.IsResultFormat(TestResultFileFormat) &&
           ".xml".Equals(Path.GetExtension(args.TestResultFilePath), StringComparison.InvariantCultureIgnoreCase);

    private TestOutcome GetOutcomeFromStepsFix(List<TestRunTestStepResult> testResultStepResults, ISpecSyncTracer tracer)
    {
        var outcomesExcluded = new[] { TestOutcome.Unknown, TestOutcome.PassedButRunAborted, TestOutcome.NotRunnable, TestOutcome.NotExecuted, TestOutcome.Disconnected, TestOutcome.Warning };
        var stepResultsConsidered = testResultStepResults.Where(r => !outcomesExcluded.Contains(r.Outcome)).ToArray();
        if (stepResultsConsidered.Length > 0)
            return stepResultsConsidered.Min(r => r.Outcome);

        var stepResult = testResultStepResults.LastOrDefault(r => r.Outcome != TestOutcome.NotExecuted)?.Outcome;
        if (stepResult != null)
            return stepResult.Value;

        tracer.TraceWarning("Not specified test result, will be treated as 'NotExecuted'");
        return TestOutcome.Unknown; // will be converted to 'NotExecuted'
    }


    protected override void ProcessTestSuite(JUnitTestSuite testSuite, LocalTestRun localTestRun, TestResultLoaderProviderArgs args)
    {
        base.ProcessTestSuite(testSuite, localTestRun, args);
        var testResult = localTestRun.TestResults.LastOrDefault();
        if (testResult != null)
        {
            var requestResult = new TestRunTestStepResult
            {
                Outcome = TestOutcome.Passed
            };
            if (!testResult.HasStepResults && 
                (testSuite.Errors > 0 || testSuite.SystemErr != null))
            {
                testResult.Outcome = TestOutcome.Failed;
                var errorMessage = GetErrorFromSystemErr(testSuite.SystemErr);
                testResult.ErrorMessage = errorMessage;
                testResult.ErrorStackTrace = testSuite.SystemErr;
                requestResult.Outcome = TestOutcome.Failed;
                requestResult.ErrorMessage = errorMessage;
            }

            testResult.StepResults.Insert(0, requestResult);

            testResult.Outcome = GetOutcomeFromStepsFix(testResult.StepResults, args.Tracer);
        }
    }

    private string? GetErrorFromSystemErr(string? systemErr)
    {
        if (systemErr == null)
            return null;
        var match = Regex.Match(systemErr, @"Error:\s*(?<message>.*)$", RegexOptions.Multiline);
        if (match.Success)
            return match.Groups["message"].Value.Trim();
        return systemErr;
    }

    public override LocalTestRun LoadTestResult(TestResultLoaderProviderArgs args)
    {
        var localTestRun = base.LoadTestResult(args);

        // For Postman/Newman tests it is not well-defined what is a test and what is a step.
        // So we will create fake merged test results for folder tests to allow better reporting and linking.

        var folderNames = localTestRun.TestResults
            .Select(td => GetFolderName(td.GetTestName()))
            .Distinct()
            .ToArray();

        var folderGroups = new List<(string GroupName, LocalTestResult[] TestResults)>();

        foreach (var folderName in folderNames)
        {
            folderGroups.Add((folderName, localTestRun.TestResults.Where(td => td.GetTestName().StartsWith(folderName)).ToArray()));
        }

        args.Tracer.TraceInformation($"Creating {folderGroups.Count} merged test results for the folders...");

        foreach (var folderGroup in folderGroups)
        {
            var mergedResult = MergeResults(folderGroup.TestResults, folderGroup.GroupName);
            localTestRun.TestResults.Add(mergedResult);
        }

        return localTestRun;
    }

    private string GetFolderName(string name)
    {
        var slashIndex = name.LastIndexOf('/');
        if (slashIndex < 0)
            return "";

        return name.Substring(0, slashIndex).Trim();
    }

    private LocalTestResult MergeResults(LocalTestResult[] testRunTestResults, string testName)
    {
        var testRunTestResult = new LocalTestResult
        {
            Name = testName,
            Duration = TimeSpan.FromMilliseconds(testRunTestResults.Where(r => r.Duration != null).Sum(r => r.Duration!.Value.TotalMilliseconds)),
            StepResults = testRunTestResults.SelectMany(r => r.StepResults).ToList(),
            Outcome = testRunTestResults.Where(r => r.Outcome != TestOutcome.NotExecuted).Min(r => r.Outcome),
            ErrorMessage = testRunTestResults.FirstOrDefault(r => r.ErrorMessage != null)?.ErrorMessage,
            ErrorStackTrace = testRunTestResults.FirstOrDefault(r => r.ErrorStackTrace != null)?.ErrorStackTrace,
        };
        return testRunTestResult;
    }
}