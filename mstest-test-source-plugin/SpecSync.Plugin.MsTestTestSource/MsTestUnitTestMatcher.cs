using System.Collections.Generic;
using SpecSync.Parsing;
using SpecSync.PluginDependency.CSharpSource;
using SpecSync.PublishTestResults;
using SpecSync.PublishTestResults.Matchers;

namespace SpecSync.Plugin.MsTestTestSource
{
    public class MsTestUnitTestMatcher : ITestRunnerResultMatcher
    {
        public bool CanProcess(TestRunnerResultMatcherArgs args)
            => args.TestFrameworkIdentifier.ToLowerInvariant().Contains("mstest");

        public string ServiceDescription => "MsTest matcher for unit tests";

        public MatchResultSelector GetLocalTestCaseResultSelector(ILocalTestCase localTestCase, ILocalTestCaseContainer localTestCaseContainer, TestRunnerResultMatcherArgs args)
        {
            var localTestMethod = (CSharpTestMethodLocalTestCase)localTestCase;
            var localFullClassName = localTestMethod.Namespace + "." + localTestMethod.ClassName;
            var localMethodName = localTestMethod.MethodName;

            return new MatchResultSelector($"<className> is '{localFullClassName}' and <methodName> is '{localMethodName}'",
                td => td.ClassName.Equals(localFullClassName) &&
                      (td.MethodName.Equals(localMethodName) || td.MethodName.StartsWith(localMethodName + "(")));
        }

        public IDictionary<string, string> GetDataRow(TestRunTestResult testResult, TestRunTestDefinition testDefinition, ILocalTestCase localTestCase, ILocalTestCaseContainer localTestCaseContainer, TestRunnerResultMatcherArgs args)
        {
            return null; // for data-driven tests the parameters could be listed here (optional)
        }
    }
}