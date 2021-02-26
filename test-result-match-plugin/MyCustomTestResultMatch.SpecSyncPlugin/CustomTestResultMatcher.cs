using System;
using SpecSync.AzureDevOps.Gherkin;
using SpecSync.AzureDevOps.PublishTestResults.Matchers;

namespace MyCustomTestResultMatch.SpecSyncPlugin
{
    public class CustomTestResultMatcher : GherkinTestRunnerResultMatcher
    {
        public override string ServiceDescription => "Custom Test Result";

        public override bool CanProcess(TestRunnerResultMatcherArgs args)
        {
            return args.TestFrameworkIdentifier == "executor://nunit3testexecutor/";
        }

        protected override MatchResultSelector GetScenarioResultSelector(ScenarioLocalTestCase scenarioLocalTestCase, FeatureFileLocalTestCaseContainer featureFileLocalTestCaseContainer, TestRunnerResultMatcherArgs args)
        {
            var scenarioName = scenarioLocalTestCase.Name;
            var featureName = featureFileLocalTestCaseContainer.Name;

            // Use scenarioLocalTestCase.IsScenarioOutline is matching needs to be done differently 
            // for scenario outlines.
            
            // The first parameter of the MatchResultSelector is a diagnostic message that helps users to 
            // understand why the matcher could not find a test.
            // To see these messages, invoke SpecSync with an additional --verbose option.

            return new MatchResultSelector($"<name> starts with '{scenarioName}'",
                td => td.Name.StartsWith(scenarioName));
        }
    }
}
