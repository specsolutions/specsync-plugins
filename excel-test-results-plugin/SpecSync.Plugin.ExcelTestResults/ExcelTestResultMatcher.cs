using SpecSync.Parsing;
using SpecSync.PublishTestResults;
using SpecSync.PublishTestResults.Matchers;

namespace SpecSync.Plugin.ExcelTestResults;

public class ExcelTestResultMatcher(ExcelResultParameters excelResultParameters) : ITestRunnerResultMatcher
{
    public virtual string ServiceDescription => "Excel Test Result";
    public virtual bool CanProcess(TestRunnerResultMatcherArgs args)
        => args.TestFrameworkIdentifier.Equals(ExcelTestResultLoader.FormatSpecifier, StringComparison.InvariantCultureIgnoreCase);


    public MatchResultSelector GetLocalTestCaseResultSelector(ILocalTestCase localTestCase,
        ISourceDocument localTestCaseContainer, TestRunnerResultMatcherArgs args)
    {
        var scenarioName = localTestCase.Name;
        var featureName = localTestCaseContainer.Name;
        var featureFileName = Path.GetFileName(localTestCaseContainer.SourceReference.ProjectRelativePath);
        var testCaseId = localTestCase.IdLink!.Id.GetNumericId();

        string? IdCellValueConverter(string cellValue)
        {
            return ExcelTestResultLoader.GetTestCaseLink(cellValue, args.TagServices)?.Id.ToString();
        }

        return 
            CombineSelectorsOr(
                CreateColumnMatch(excelResultParameters.TestCaseIdColumnName, testCaseId.ToString(), IdCellValueConverter, resultIfNotSpecified: false),
                CombineSelectorsAnd(
                    CreateColumnMatch(excelResultParameters.FeatureFileColumnName, featureFileName),
                    CreateColumnMatch(excelResultParameters.FeatureColumnName, featureName),
                    CreateColumnMatch(excelResultParameters.ScenarioColumnName, scenarioName),
                    CreateColumnMatch(excelResultParameters.TestCaseIdColumnName, testCaseId.ToString(), IdCellValueConverter)
                ));
    }

    private MatchResultSelector CreateColumnMatch(string columnName, string value, Func<string, string?>? cellValueConverter = null, bool resultIfNotSpecified = true)
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

    private bool EqualsToStringIfSpecified(LocalTestResult localTestResult, string columnName, string value, Func<string, string?>? cellValueConverter, bool resultIfNotSpecified)
    {
        var cellValue = GetCellValue<string>(localTestResult, columnName);
        if (cellValueConverter != null)
            cellValue = cellValueConverter(cellValue);
        if (string.IsNullOrEmpty(cellValue))
            return resultIfNotSpecified;
        return value.Equals(cellValue, StringComparison.OrdinalIgnoreCase);
    }

    private T GetCellValue<T>(LocalTestResult localTestResult, string columnName)
    {
        var objValue = localTestResult.GetProperty<object>(columnName);
        if (objValue is T value)
            return value;
        return (T)Convert.ChangeType(objValue, typeof(T));
    }

    public IDictionary<string, string>? GetInvocationArguments(LocalTestResult testResult, ILocalTestCase localTestCase, ISourceDocument sourceDocument, TestRunnerResultMatcherArgs args)
    {
        return null;
    }
}