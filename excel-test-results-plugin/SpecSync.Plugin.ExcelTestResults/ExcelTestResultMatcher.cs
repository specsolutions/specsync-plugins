using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpecSync.Parsing;
using SpecSync.PublishTestResults;
using SpecSync.PublishTestResults.Matchers;

namespace SpecSync.Plugin.ExcelTestResults
{
    public class ExcelTestResultMatcher : ITestRunnerResultMatcher
    {
        private readonly ExcelResultParameters _excelResultParameters;

        public ExcelTestResultMatcher(ExcelResultParameters excelResultParameters)
        {
            _excelResultParameters = excelResultParameters;
        }

        public virtual string ServiceDescription => "Excel Test Result";
        public virtual bool CanProcess(TestRunnerResultMatcherArgs args)
            => args.TestFrameworkIdentifier.Equals(ExcelTestResultLoader.FormatSpecifier, StringComparison.InvariantCultureIgnoreCase);


        public MatchResultSelector GetLocalTestCaseResultSelector(ILocalTestCase localTestCase,
            ILocalTestCaseContainer localTestCaseContainer, TestRunnerResultMatcherArgs args)
        {
            //TODO: replace by ltc.Name with SpecSync v3.5
            string GetCompatibilityLocalTestCaseName(ILocalTestCase ltc)
            {
                return (string)ltc.GetType().GetProperty("Name")?.GetValue(ltc);
            }

            var scenarioName = GetCompatibilityLocalTestCaseName(localTestCase);
            var featureName = localTestCaseContainer.Name;
            var featureFileName = Path.GetFileName(localTestCaseContainer.SourceFile.ProjectRelativePath);
            var testCaseId = localTestCase.TestCaseLink.TestCaseId.GetNumericId();

            return CombineSelectors(
                CreateColumnMatch(_excelResultParameters.FeatureFileColumnName, featureFileName),
                CreateColumnMatch(_excelResultParameters.FeatureColumnName, featureName),
                CreateColumnMatch(_excelResultParameters.ScenarioColumnName, scenarioName),
                CreateNumericColumnMatch(_excelResultParameters.TestCaseIdColumnName, testCaseId, _excelResultParameters.TestCaseIdValueRegex)
            );
        }

        private MatchResultSelector CreateColumnMatch(string columnName, string value, string valueRegex = null)
        {
            return new MatchResultSelector($"[{columnName}] is '{value}' (if specified)",
                td => EqualsToStringIfSpecified(td, columnName, value, valueRegex));
        }

        private MatchResultSelector CreateNumericColumnMatch(string columnName, int value, string valueRegex = null)
        {
            return CreateColumnMatch(columnName, value.ToString(), valueRegex);
        }

        private MatchResultSelector CombineSelectors(params MatchResultSelector[] selectors)
        {
            var validSelectors = selectors.Where(s => s != null).ToArray();
            return new MatchResultSelector(
                string.Join(" and ", validSelectors.Select(s => s.DiagMessage)),
                td => validSelectors.All(s => s.Func(td))
            );
        }

        private bool EqualsToStringIfSpecified(TestRunTestDefinition testDefinition, string columnName, string value, string valueRegex)
        {
            var cellValue = CellValueConverter.Convert(GetCellValue<string>(testDefinition, columnName), valueRegex);
            return string.IsNullOrEmpty(cellValue) || value.Equals(cellValue, StringComparison.OrdinalIgnoreCase);
        }

        private T GetCellValue<T>(TestRunTestDefinition testDefinition, string columnName)
        {
            var objValue = testDefinition.Results.First().GetProperty<object>(columnName);
            if (objValue is T value)
                return value;
            return (T)Convert.ChangeType(objValue, typeof(T));
        }

        public IDictionary<string, string> GetDataRow(TestRunTestResult testResult, TestRunTestDefinition testDefinition, ILocalTestCase localTestCase, ILocalTestCaseContainer localTestCaseContainer, TestRunnerResultMatcherArgs args)
        {
            return null;
        }
    }
}
