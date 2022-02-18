using System.Collections.Generic;
using SpecSync.AzureDevOps.Parsing;
using SpecSync.AzureDevOps.PublishTestResults;
using SpecSync.AzureDevOps.PublishTestResults.Matchers;

namespace MsTestTestSource.SpecSyncPlugin
{
    public class MsTestUnitTestMatcher : ITestRunnerResultMatcher
    {
        public bool CanProcess(TestRunnerResultMatcherArgs args)
            => args.TestFrameworkIdentifier.ToLowerInvariant().Contains("mstest");

        public string ServiceDescription => "MsTest matcher for unit tests";

        public MatchResultSelector GetLocalTestCaseResultSelector(ILocalTestCase localTestCase, ILocalTestCaseContainer localTestCaseContainer, TestRunnerResultMatcherArgs args)
        {
            return new MatchResultSelector($"<className> is '{localTestCaseContainer.Name}' and <methodName> is '{localTestCase.Name}'",
                td => td.MethodName.Equals(localTestCase.Name) && td.ClassName.Equals(localTestCaseContainer.Name));
        }

        public IDictionary<string, string> GetDataRow(TestRunTestResult testResult, TestRunTestDefinition testDefinition, ILocalTestCase localTestCase, ILocalTestCaseContainer localTestCaseContainer, TestRunnerResultMatcherArgs args)
        {
            return null; // for data-driven tests the parameters could be listed here (optional)
        }
    }
}