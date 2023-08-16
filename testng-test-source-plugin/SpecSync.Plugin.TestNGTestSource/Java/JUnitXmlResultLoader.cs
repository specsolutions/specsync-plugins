using System;
using System.IO;
using System.Linq;
using SpecSync.PublishTestResults;
using SpecSync.PublishTestResults.Loaders;
using SpecSync.Tracing;

namespace SpecSync.Plugin.TestNGTestSource.Java;

public class JUnitXmlResultLoader : SpecSync.PublishTestResults.Loaders.JUnitXmlResultLoader
{
    public const string JUnitXml = nameof(JUnitXml);

    protected override string TestFrameworkIdentifier => "executor://java/junit-xml";

    public override string ServiceDescription => $"{JUnitXml}: JUnit XML result";

    public override bool CanProcess(TestResultLoaderProviderArgs args) => 
        args.TestResultConfiguration.IsFileFormat(JUnitXml) && 
        ".xml".Equals(Path.GetExtension(args.TestResultFilePath), StringComparison.InvariantCultureIgnoreCase);

    public override LocalTestRun LoadTestResult(TestResultLoaderProviderArgs args)
    {
        var loadTestResult = base.LoadTestResult(args);

        // The JUnit XML results for TestNG provide the same "name" for all results of a DataDriven test.
        // This would cause SpecSync to identify it as re-run attempt wrongly, so we modify the names by adding a counter.
        var multiRunDefinitions =
            loadTestResult.TestDefinitions
                .Where(td => !td.Name.Contains("(") && !td.Name.Contains("[") && td.Results.Count == 1)
                .GroupBy(td => (td.ClassName, td.Name, td.Results[0].Name))
                .Where(g => g.Count() > 1)
                .ToArray();
        foreach (var grouping in multiRunDefinitions)
        {       
            int counter = 0;
            foreach (var testDefinition in grouping)
            {
                testDefinition.Results[0].Name = $"{testDefinition.Results[0].Name}({++counter})";
            }
        }

        return loadTestResult;
    }

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
}
