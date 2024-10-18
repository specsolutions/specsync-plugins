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

            string IdCellValueConverter(string cellValue)
            {
                return ExcelTestResultLoader.GetTestCaseLink(cellValue, args.TagServices)?.TestCaseId.ToString();
            }

            return 
                CombineSelectorsOr(
                    CreateColumnMatch(_excelResultParameters.TestCaseIdColumnName, testCaseId.ToString(), IdCellValueConverter, resultIfNotSpecified: false),
                    CombineSelectorsAnd(
                        CreateColumnMatch(_excelResultParameters.FeatureFileColumnName, featureFileName),
                        CreateColumnMatch(_excelResultParameters.FeatureColumnName, featureName),
                        CreateColumnMatch(_excelResultParameters.ScenarioColumnName, scenarioName),
                        CreateColumnMatch(_excelResultParameters.TestCaseIdColumnName, testCaseId.ToString(), IdCellValueConverter)
                ));
        }

        private MatchResultSelector CreateColumnMatch(string columnName, string value, Func<string, string> cellValueConverter = null, bool resultIfNotSpecified = true)
        {
            return new MatchResultSelector($"[{columnName}] is '{value}' (if specified)",
                td => EqualsToStringIfSpecified(td, columnName, value, cellValueConverter, resultIfNotSpecified));
        }

        private MatchResultSelector CombineSelectorsAnd(params MatchResultSelector[] selectors)
        {
            var validSelectors = selectors.Where(s => s != null).ToArray();
            return new MatchResultSelector(
                string.Join(" and ", validSelectors.Select(s => s.DiagMessage)),
                td => validSelectors.All(s => s.Func(td))
            );
        }

        private MatchResultSelector CombineSelectorsOr(params MatchResultSelector[] selectors)
        {
            var validSelectors = selectors.Where(s => s != null).ToArray();
            return new MatchResultSelector(
                string.Join(" or ", validSelectors.Select(s => $"({s.DiagMessage})")),
                td => validSelectors.Any(s => s.Func(td))
            );
        }

        private bool EqualsToStringIfSpecified(TestRunTestDefinition testDefinition, string columnName, string value, Func<string, string> cellValueConverter, bool resultIfNotSpecified)
        {
            var cellValue = GetCellValue<string>(testDefinition, columnName);
            if (cellValueConverter != null)
                cellValue = cellValueConverter(cellValue);
            if (string.IsNullOrEmpty(cellValue))
                return resultIfNotSpecified;
            return value.Equals(cellValue, StringComparison.OrdinalIgnoreCase);
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
