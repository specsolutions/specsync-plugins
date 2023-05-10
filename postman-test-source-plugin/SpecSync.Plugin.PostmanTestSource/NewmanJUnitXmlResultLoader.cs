using SpecSync.PublishTestResults.Loaders;
using SpecSync.PublishTestResults;
using SpecSync.Tracing;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SpecSync.Utils;
using System.Xml.Serialization;

namespace SpecSync.Plugin.PostmanTestSource;

public class NewmanJUnitXmlResultLoader : JUnitXmlTestCaseAsStepResultLoader
{
    public const string TestResultFileFormat = "NewmanJUnitXml";

    protected override string TestFrameworkIdentifier => NewmanJUnitXmlResultMatcher.TestFrameworkIdentifier;
    public override string ServiceDescription => $"{TestResultFileFormat}: Postman Newman JUnit XML result";

    public override bool CanProcess(TestResultLoaderProviderArgs args)
        => args.TestResultConfiguration.IsFileFormat(TestResultFileFormat) &&
           ".xml".Equals(Path.GetExtension(args.TestResultFilePath), StringComparison.InvariantCultureIgnoreCase);

    //TODO: this override can be removed when target SpecSync 3.5 as it was implemented in 3.4.4.
    protected override TestOutcome GetOutcome(string testCaseStatus, JUnitTestCase testCase, ISpecSyncTracer tracer)
    {
        if (string.IsNullOrEmpty(testCaseStatus))
        {
            if (testCase.Failure != null || testCase.Error != null)
                return TestOutcome.Failed;

            return TestOutcome.Passed;
        }

        return base.GetOutcome(testCaseStatus, testCase, tracer);
    }

    private TestOutcome GetOutcomeFromStepsFix(List<TestRunTestStepResult> testResultStepResults, ISpecSyncTracer tracer)
    {
        var outcomesExcluded = new[] { TestOutcome.Unknown, TestOutcome.PassedButRunAborted, TestOutcome.NotRunnable, TestOutcome.NotExecuted, TestOutcome.Disconnected, TestOutcome.Warning };
        var stepResultsConsidered = testResultStepResults.Where(r => !outcomesExcluded.Contains(r.Outcome)).ToArray();
        if (stepResultsConsidered.Length > 0)
            return stepResultsConsidered.Min(r => r.Outcome);

        var stepResult = testResultStepResults?.LastOrDefault(r => r.Outcome != TestOutcome.NotExecuted)?.Outcome;
        if (stepResult != null)
            return stepResult.Value;

        tracer?.TraceWarning("Not specified test result, will be treated as 'NotExecuted'");
        return TestOutcome.Unknown; // will be converted to 'NotExecuted'
    }


    protected override void ProcessTestSuite(JUnitTestSuite testSuite, LocalTestRun result, TestResultLoaderProviderArgs args)
    {
        base.ProcessTestSuite(testSuite, result, args);
        var testDefinition = result.TestDefinitions.LastOrDefault();
        if (testDefinition != null)
        {
            var requestResult = new TestRunTestStepResult
            {
                Outcome = TestOutcome.Passed
            };
            var testResult = testDefinition.Results[0];
            if (!testResult.HasStepResults && testSuite is ExtendedJUnitTestSuite extendedTestSuite && 
                (extendedTestSuite.Errors > 0 || extendedTestSuite.SystemErr != null))
            {
                testResult.Outcome = TestOutcome.Failed;
                var errorMessage = GetErrorFromSystemErr(extendedTestSuite.SystemErr);
                testResult.ErrorMessage = errorMessage;
                testResult.ErrorStackTrace = extendedTestSuite.SystemErr;
                requestResult.Outcome = TestOutcome.Failed;
                requestResult.ErrorMessage = errorMessage;
            }

            testResult.StepResults.Insert(0, requestResult);

            testDefinition.Results[0].Outcome = GetOutcomeFromStepsFix(testDefinition.Results[0].StepResults, args.Tracer);
        }
    }

    private string GetErrorFromSystemErr(string systemErr)
    {
        if (systemErr == null)
            return null;
        var match = Regex.Match(systemErr, @"Error:\s*(?<message>.*)$", RegexOptions.Multiline);
        if (match.Success)
            return match.Groups["message"].Value.Trim();
        return systemErr;
    }

    public class ExtendedJUnitTestSuite : JUnitTestSuite
    {
        [XmlAttribute("errors")]
        public int Errors { get; set; }

        [XmlElement("system-err")]
        public string SystemErr { get; set; }
    }

    protected virtual JUnitTestSuites DeserializeFromTestSuites(string xmlContent)
    {
        var xmlAttributeOverrides = new XmlAttributeOverrides();
        xmlAttributeOverrides.Add(typeof(JUnitTestSuites), nameof(JUnitTestSuites.TestSuites), new XmlAttributes
        {
            XmlElements = { new XmlElementAttribute("testsuite", typeof(ExtendedJUnitTestSuite)) }
        });
        var serializer = new XmlSerializer(typeof(JUnitTestSuites), xmlAttributeOverrides);
        var reader = new StringReader(xmlContent);
        return (JUnitTestSuites)serializer.Deserialize(reader);
    }

    public override LocalTestRun LoadTestResult(TestResultLoaderProviderArgs args)
    {
        var result = BaseLoadTestResult(args);

        var folderNames = result.TestDefinitions
            .Select(td => GetFolderName(td.Name))
            .Distinct()
            .ToArray();

        var folderGroups = new List<(string GroupName, TestRunTestDefinition[] TestDefinitions)>();

        foreach (var folderName in folderNames)
        {
            folderGroups.Add((folderName, result.TestDefinitions.Where(td => td.Name.StartsWith(folderName)).ToArray()));
        }

        args.Tracer.TraceInformation($"Creating {folderGroups.Count} merged test results for the folders...");

        foreach (var folderGroup in folderGroups)
        {
            var testDefinition = new TestRunTestDefinition
            {
                Name = folderGroup.GroupName,
                ClassName = folderGroup.TestDefinitions.First().ClassName,
                Results =
                {
                    MergeResults(folderGroup.TestDefinitions.SelectMany(td => td.Results).ToArray(), folderGroup.GroupName)
                }
            };
            result.TestDefinitions.Add(testDefinition);
        }

        return result;
    }

    private string GetFolderName(string name)
    {
        var slashIndex = name.LastIndexOf('/');
        if (slashIndex < 0)
            return null;

        return name.Substring(0, slashIndex).Trim();
    }

    private TestRunTestResult MergeResults(TestRunTestResult[] testRunTestResults, string testName)
    {
        var testRunTestResult = new TestRunTestResult
        {
            Name = testName,
            Duration = TimeSpan.FromMilliseconds(testRunTestResults.Where(r => r.Duration != null).Sum(r => r.Duration.Value.TotalMilliseconds)),
            StepResults = testRunTestResults.SelectMany(r => r.StepResults).ToList(),
            Outcome = testRunTestResults.Where(r => r.Outcome != TestOutcome.NotExecuted).Min(r => r.Outcome),
            ErrorMessage = testRunTestResults.FirstOrDefault(r => r.ErrorMessage != null)?.ErrorMessage,
            ErrorStackTrace = testRunTestResults.FirstOrDefault(r => r.ErrorStackTrace != null)?.ErrorStackTrace,
        };
        return testRunTestResult;
    }

    #region Allow overriding deseralization

    private LocalTestRun BaseLoadTestResult(TestResultLoaderProviderArgs args)
    {
        var xmlContent = File.ReadAllText(args.TestResultFilePath).Trim();
        JUnitTestSuites testSuites;
        try
        {
            testSuites = DeserializeFromTestSuites(xmlContent);
        }
        catch (Exception ex)
        {
            throw new SpecSyncException($"Unable to parse test result file: {ex.Message}", ex);
        }

        var result = new LocalTestRun
        {
            Name = testSuites.Name,
            TestFrameworkIdentifier = TestFrameworkIdentifier
        };

        foreach (var testSuite in testSuites.TestSuites)
        {
            ProcessTestSuite(testSuite, result, args);
        }

        return result;
    }

    #endregion
}