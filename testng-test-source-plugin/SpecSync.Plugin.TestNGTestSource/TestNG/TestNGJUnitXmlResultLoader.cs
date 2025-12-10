using SpecSync.Plugin.TestNGTestSource.Java;
using SpecSync.PublishTestResults;
using SpecSync.PublishTestResults.Loaders;

namespace SpecSync.Plugin.TestNGTestSource.TestNG;

public class TestNGJUnitXmlResultLoader : JUnitXmlResultLoader
{
    public override string ServiceDescription => $"{JUnitXml}: JUnit XML result for TestNG";

    public override LocalTestRun LoadTestResult(TestResultLoaderProviderArgs args)
    {
        var localTestRun = base.LoadTestResult(args);

        // The JUnit XML results for TestNG provide the same "name" for all results of a DataDriven test.
        // This would cause SpecSync to identify it as re-run attempt wrongly, so we modify the names by adding a counter.
        var multiRunResults =
            localTestRun.TestResults
                .Where(td => !td.GetTestName().Contains("(") && !td.GetTestName().Contains("["))
                .GroupBy(td => (td.ClassName, td.GetTestName(), td.GetName()))
                .Where(g => g.Count() > 1)
                .ToArray();
        foreach (var grouping in multiRunResults)
        {
            int counter = 0;
            foreach (var testResult in grouping)
            {
                testResult.Name = $"{testResult.Name}({++counter})";
            }
        }

        return localTestRun;
    }

}