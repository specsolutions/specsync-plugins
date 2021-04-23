using System;
using SpecSync.AzureDevOps.Gherkin;
using SpecSync.AzureDevOps.PublishTestResults.Matchers;

namespace MsTestTestSource.SpecSyncPlugin
{
    public class CustomTestResultMatcher : GherkinTestRunnerResultMatcher
    {
        public override string ServiceDescription => "Custom Test Result";

        public override bool CanProcess(TestRunnerResultMatcherArgs args)
        {
            // for using this matcher only for a specific test runner, you can use the args.TestFrameworkIdentifier, like:
            // return args.TestFrameworkIdentifier.IndexOf("mstest", StringComparison.InvariantCultureIgnoreCase) >= 0;
            return true;
        }

        protected override MatchResultSelector GetScenarioResultSelector(ScenarioLocalTestCase scenarioLocalTestCase, FeatureFileLocalTestCaseContainer featureFileLocalTestCaseContainer, TestRunnerResultMatcherArgs args)
        {
            var scenarioName = scenarioLocalTestCase.Name;
            var featureName = featureFileLocalTestCaseContainer.Name;

            // Use scenarioLocalTestCase.IsScenarioOutline is matching needs to be done differently 
            // for scenario outlines.

            // The first parameter of the MatchResultSelector is a diagnostic message that helps users to 
            // understand why the matcher could not find a test.
            // To see these messages, invoke SpecSync with an additional --diag option.

            return new MatchResultSelector($"<className> ends with '{featureName}' and <name> is '{scenarioName}'",
                td => td.ClassName.EndsWith(featureName) &&
                      td.Name == scenarioName);
        }
    }
}
