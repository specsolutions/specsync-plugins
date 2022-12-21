using System;
using System.IO;
using System.Linq;
using SpecSync.Gherkin;
using SpecSync.PublishTestResults;
using SpecSync.PublishTestResults.Matchers;

namespace SpecSync.Plugin.ExcelTestResults
{
    public class ExcelTestResultMatcher : GherkinTestRunnerResultMatcher
    {
        private readonly ExcelResultParameters _excelResultParameters;

        public ExcelTestResultMatcher(ExcelResultParameters excelResultParameters)
        {
            _excelResultParameters = excelResultParameters;
        }

        public override string ServiceDescription => "Excel Test Result";

        protected override MatchResultSelector GetScenarioResultSelector(ScenarioLocalTestCase scenarioLocalTestCase, FeatureFileLocalTestCaseContainer featureFileLocalTestCaseContainer, TestRunnerResultMatcherArgs args)
        {
            var scenarioName = scenarioLocalTestCase.Name;
            var featureName = featureFileLocalTestCaseContainer.Name;
            var featureFileName = Path.GetFileName(featureFileLocalTestCaseContainer.SourceFile.ProjectRelativePath);
            var testCaseId = scenarioLocalTestCase.TestCaseLink.TestCaseId.GetNumericId();

            return CombineSelectors(
                CreateColumnMatch(_excelResultParameters.FeatureFileColumnName, featureFileName),
                CreateColumnMatch(_excelResultParameters.FeatureColumnName, featureName),
                CreateColumnMatch(_excelResultParameters.ScenarioColumnName, scenarioName),
                CreateNumericColumnMatch(_excelResultParameters.TestCaseIdColumnName, testCaseId)
            );
        }

        private MatchResultSelector CreateColumnMatch(string columnName, string value)
        {
            return new MatchResultSelector($"[{columnName}] is '{value}' (if specified)",
                td => EqualsToStringIfSpecified(td, columnName, value));
        }

        private MatchResultSelector CreateNumericColumnMatch(string columnName, int value)
        {
            return CreateColumnMatch(columnName, value.ToString());
        }

        private MatchResultSelector CombineSelectors(params MatchResultSelector[] selectors)
        {
            var validSelectors = selectors.Where(s => s != null).ToArray();
            return new MatchResultSelector(
                string.Join(" and ", validSelectors.Select(s => s.DiagMessage)),
                td => validSelectors.All(s => s.Func(td))
            );
        }

        private bool EqualsToStringIfSpecified(TestRunTestDefinition testDefinition, string columnName, string value)
        {
            var cellValue = GetCellValue<string>(testDefinition, columnName);
            return string.IsNullOrEmpty(cellValue) || value.Equals(cellValue, StringComparison.OrdinalIgnoreCase);
        }

        private T GetCellValue<T>(TestRunTestDefinition testDefinition, string columnName)
        {
            return testDefinition.Results.First().GetProperty<T>(columnName);
        }

        public override bool CanProcess(TestRunnerResultMatcherArgs args)
            => args.TestFrameworkIdentifier.Equals(ExcelTestResultLoader.FormatSpecifier, StringComparison.InvariantCultureIgnoreCase);
    }
}
