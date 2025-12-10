using System.Data;
using ExcelDataReader;
using SpecSync.Analyzing;
using SpecSync.Parsing;
using SpecSync.PublishTestResults;
using SpecSync.PublishTestResults.Loaders;
using SpecSync.Synchronization;
using SpecSync.Utils;

namespace SpecSync.Plugin.ExcelTestResults;

public class ExcelTestResultLoader(ExcelResultParameters excelResultParameters) : ITestResultLoader
{
    public const string FormatSpecifier = "Excel";

    public string ServiceDescription => $"{FormatSpecifier}: Excel Test Result";

    public bool CanProcess(TestResultLoaderProviderArgs args)
    {
        return
            args.TestResultConfiguration.IsResultFormat(FormatSpecifier) &&
            args.TestResultFilePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);
    }

    public LocalTestRun LoadTestResult(TestResultLoaderProviderArgs args)
    {
        excelResultParameters.Verify();

        var testResultTable = LoadExcelDataTable(args.TestResultFilePath, excelResultParameters.TestResultSheetName);

        var localTestRun = new LocalTestRun
        {
            TestFrameworkIdentifier = FormatSpecifier,
            Name = $"{Path.GetFileName(args.TestResultFilePath)} - {testResultTable.TableName}"
        };

        for (int rowIndex = 0; rowIndex < testResultTable.Rows.Count; rowIndex++)
        {
            DataRow row = testResultTable.Rows[rowIndex];
            int rowNumber = rowIndex + 2; // rowIndex is 0-indexed, include header row

            var localTestResult = new LocalTestResult
            {
                ClassName = GetClassName(row),
                MethodName = GetMethodName(row, args.TagServices),
                TestName = GetName(row, args.TagServices),
                Name = $"Excel row {rowNumber}",
                Outcome = GetOutcome(row, rowNumber),
                ErrorMessage = GetErrorMessage(row)
            };

            if (IsEmptyTestDefinition(localTestResult))
            {
                args.Tracer.LogVerbose($"Row {rowNumber} does not contain test reference. Skipping.");
                continue;
            }

            foreach (DataColumn column in testResultTable.Columns)
            {
                localTestResult.AddProperty(column.ColumnName, row[column]);
            }
            localTestRun.TestResults.Add(localTestResult);
        }

        return localTestRun;
    }

    private bool IsEmptyTestDefinition(LocalTestResult localTestResult)
    {
        return string.IsNullOrWhiteSpace(localTestResult.ClassName) &&
               string.IsNullOrWhiteSpace(localTestResult.MethodName) &&
               string.IsNullOrWhiteSpace(localTestResult.Name);
    }

    protected virtual TestOutcome GetOutcome(DataRow row, int rowNumber)
    {
        var outcomeValue = GetCellValue(row, excelResultParameters.OutcomeColumnName);
        return ConvertOutcome(outcomeValue, rowNumber);
    }

    protected virtual TestOutcome ConvertOutcome(string? outcomeValue, int rowNumber)
    {
        if (string.IsNullOrWhiteSpace(outcomeValue))
            return TestOutcome.NotExecuted;

        var convertedOutcomeValue = excelResultParameters.OutcomeMappings.TryGetValue(outcomeValue!, out var mappedValue) ? 
                mappedValue : outcomeValue;

        if (Enum.TryParse<TestOutcome>(convertedOutcomeValue, true, out var outcome))
            return outcome;

        throw new SpecSyncException($"Invalid outcome value or the column {excelResultParameters.OutcomeColumnName} is not defined at row {rowNumber}: '{outcomeValue}'. Possible values: {string.Join(", ", Enum.GetNames(typeof(TestOutcome)))}. You can map custom values using the 'OutcomeMapping' parameter in 'PASS=Passed,FAIL=Failed' format.");
    }

    private bool TryGetCellValue(DataRow row, string columnName, out string? value)
    {
        if (row.Table.Columns.Contains(columnName))
        {
            value = row[columnName]?.ToString();
            return true;
        }

        value = null;
        return false;
    }

    private string? GetCellValue(DataRow row, string columnName)
    {
        if (TryGetCellValue(row, columnName, out var value))
            return value;
        return null;
    }

    private string? GetErrorMessage(DataRow row)
    {
        return GetCellValue(row, excelResultParameters.ErrorMessageColumnName);
    }

    private string GetMethodName(DataRow row, ITagServices tagServices)
    {
        return GetTestCaseId(row, tagServices) ??
               GetCellValue(row, excelResultParameters.ScenarioColumnName) ??
               string.Empty;
    }

    private string? GetTestCaseId(DataRow row, ITagServices tagServices)
    {
        var cellValue = GetCellValue(row, excelResultParameters.TestCaseIdColumnName);
        return GetTestCaseLink(cellValue, tagServices)?.Id.ToString();
    }

    public static IdLink? GetTestCaseLink(string? idCellValue, ITagServices tagServices)
    {
        if (string.IsNullOrWhiteSpace(idCellValue))
            return null;

        var tags = new ILocalArtifactTag[] { new LocalArtifactTag(idCellValue!) };
        var testCaseLink = tagServices.GetTestCaseLinkFromTags(tags);

        if (testCaseLink == null)
        {
            var testCaseId = int.Parse(idCellValue);
            testCaseLink = new IdLink(TestCaseIdentifier.CreateExistingFromNumericId(testCaseId), "");
        }

        return testCaseLink;
    }

    private string GetClassName(DataRow row)
    {
        return GetCellValue(row, excelResultParameters.FeatureFileColumnName) ??
               GetCellValue(row, excelResultParameters.FeatureColumnName) ??
               string.Empty;
    }

    private string GetName(DataRow row, ITagServices tagServices)
    {
        return GetCellValue(row, excelResultParameters.TestNameColumnName) ??
               GetCellValue(row, excelResultParameters.ScenarioColumnName) ??
               GetTestCaseId(row, tagServices) ??
               string.Empty;
    }

    public DataTable LoadExcelDataTable(string filePath, string? sheetName = null)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);

        var result = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
            {
                UseHeaderRow = true
            }
        });

        var resultTable = sheetName == null ? result.Tables[0] : result.Tables[sheetName];
        return resultTable;
    }
}