using System;
using System.IO;
using SpecSync.AzureDevOps.PublishTestResults;
using SpecSync.AzureDevOps.PublishTestResults.Loaders;

namespace MsTestTestSource.SpecSyncPlugin
{
    /* processes a custom test result file in format:
     
     MyTest1,Passed
     MyTest2,Passed
     MyTest3,Failed
     
     */

    public class MyTestResultLoader : ITestResultLoader
    {
        public string ServiceDescription => "CustomTXT: Custom TXT loader";
        public bool CanProcess(TestResultLoaderProviderArgs args)
            => args.TestResultConfiguration.IsFileFormat("CustomTXT");

        public LocalTestRun LoadTestResult(TestResultLoaderProviderArgs args)
        {
            var lines = File.ReadAllLines(args.TestResultFilePath);
            var result = new LocalTestRun
            {
                Name = "Custom test Run",
                TestFrameworkIdentifier = "Custom"
            };
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                var testDefinition = new TestRunTestDefinition
                {
                    ClassName = "class",
                    MethodName = "method",
                    Name = parts[0],
                };
                testDefinition.Results.Add(new TestRunTestResult
                {
                    Outcome = (TestOutcome)Enum.Parse(typeof(TestOutcome), parts[1], true)
                });
                result.TestDefinitions.Add(testDefinition);
            }

            return result;
        }
    }
}