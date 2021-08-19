using System;
using System.IO;
using System.Linq;
using SpecSync.AzureDevOps.Gherkin;
using SpecSync.AzureDevOps.PublishTestResults;
using SpecSync.AzureDevOps.PublishTestResults.Matchers;

namespace ExcelTestResults.SpecSyncPlugin
{
    public class ExcelTestResultMatcher : GherkinTestRunnerResultMatcher
    {
        private readonly ExcelResultSpecification _excelResultSpecification;

        public ExcelTestResultMatcher(ExcelResultSpecification excelResultSpecification)
        {
            _excelResultSpecification = excelResultSpecification;
        }

        public override string ServiceDescription => "Excel Test Result";

        protected override MatchResultSelector GetScenarioResultSelector(ScenarioLocalTestCase scenarioLocalTestCase, FeatureFileLocalTestCaseContainer featureFileLocalTestCaseContainer, TestRunnerResultMatcherArgs args)
        {
            var scenarioName = scenarioLocalTestCase.Name;
            var featureName = featureFileLocalTestCaseContainer.Name;
            var featureFileName = Path.GetFileName(featureFileLocalTestCaseContainer.SourceFile.ProjectRelativePath);
            var testCaseId = scenarioLocalTestCase.TestCaseLink.TestCaseId;

            return CombineSelectors(
                CreateColumnMatch(_excelResultSpecification.FeatureFileColumnName, featureFileName),
                CreateColumnMatch(_excelResultSpecification.FeatureColumnName, featureName),
                CreateColumnMatch(_excelResultSpecification.ScenarioColumnName, scenarioName),
                CreateNumericColumnMatch(_excelResultSpecification.TestCaseIdColumnName, testCaseId)
            );
        }

        private MatchResultSelector CreateColumnMatch(string columnName, string value)
        {
            if (string.IsNullOrEmpty(columnName))
                return null;
            return new MatchResultSelector($"[{columnName}] is '{value}'",
                td => value.Equals(GetCellValue<string>(td, columnName), StringComparison.OrdinalIgnoreCase));
        }

        private MatchResultSelector CreateNumericColumnMatch(string columnName, int value)
        {
            if (string.IsNullOrEmpty(columnName))
                return null;
            return new MatchResultSelector($"[{columnName}] is {value}",
                td => value.Equals(GetCellValue<int>(td, columnName)));
        }

        private MatchResultSelector CombineSelectors(params MatchResultSelector[] selectors)
        {
            var validSelectors = selectors.Where(s => s != null).ToArray();
            return new MatchResultSelector(
                string.Join(" and ", validSelectors.Select(s => s.DiagMessage)),
                td => validSelectors.All(s => s.Func(td))
            );
        }

        private T GetCellValue<T>(TestRunTestDefinition testDefinition, string columnName)
        {
            return testDefinition.Results.First().GetProperty<T>(columnName);
        }

        public override bool CanProcess(TestRunnerResultMatcherArgs args)
            => args.TestFrameworkIdentifier.Equals(ExcelTestResultLoader.FormatSpecifier, StringComparison.InvariantCultureIgnoreCase);
    }
}
