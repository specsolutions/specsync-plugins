using System;
using System.Data;
using System.IO;
using ExcelDataReader;
using SpecSync.Analyzing;
using SpecSync.Parsing;
using SpecSync.PublishTestResults;
using SpecSync.PublishTestResults.Loaders;
using SpecSync.Synchronization;
using SpecSync.Utils;

namespace SpecSync.Plugin.ExcelTestResults;

public class ExcelTestResultLoader : ITestResultLoader
{
    public const string FormatSpecifier = "Excel";
    private readonly ExcelResultParameters _excelResultParameters;

    public ExcelTestResultLoader(ExcelResultParameters excelResultParameters)
    {
        _excelResultParameters = excelResultParameters;
    }

    public string ServiceDescription => $"{FormatSpecifier}: Excel Test Result";

    public bool CanProcess(TestResultLoaderProviderArgs args)
    {
        return
            args.TestResultConfiguration.IsFileFormat(FormatSpecifier) &&
            args.TestResultFilePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);
    }

    public LocalTestRun LoadTestResult(TestResultLoaderProviderArgs args)
    {
        _excelResultParameters.Verify();

        var testResultTable = LoadExcelDataTable(args.TestResultFilePath, _excelResultParameters.TestResultSheetName);

        var localTestRun = new LocalTestRun
        {
            TestFrameworkIdentifier = FormatSpecifier,
            Name = $"{Path.GetFileName(args.TestResultFilePath)} - {testResultTable.TableName}"
        };

        for (int rowIndex = 0; rowIndex < testResultTable.Rows.Count; rowIndex++)
        {
            DataRow row = testResultTable.Rows[rowIndex];
            int rowNumber = rowIndex + 2; // rowIndex is 0-indexed, include header row
            var testDefinition = new TestRunTestDefinition
            {
                ClassName = GetClassName(row),
                MethodName = GetMethodName(row, args.TagServices),
                Name = GetName(row, args.TagServices),
            };

            if (IsEmptyTestDefinition(testDefinition))
            {
                args.Tracer.LogVerbose($"Row {rowNumber} does not contain test reference. Skipping.");
                continue;
            }

            var testRunTestResult = new TestRunTestResult
            {
                Name = $"Excel row {rowNumber}",
                Outcome = GetOutcome(row, rowNumber),
                ErrorMessage = GetErrorMessage(row)
            };
            foreach (DataColumn column in testResultTable.Columns)
            {
                testRunTestResult.AddProperty(column.ColumnName, row[column]);
            }
            testDefinition.Results.Add(testRunTestResult);
            localTestRun.TestDefinitions.Add(testDefinition);
        }

        return localTestRun;
    }

    private bool IsEmptyTestDefinition(TestRunTestDefinition testDefinition)
    {
        return string.IsNullOrWhiteSpace(testDefinition.ClassName) &&
               string.IsNullOrWhiteSpace(testDefinition.MethodName) &&
               string.IsNullOrWhiteSpace(testDefinition.Name);
    }

    protected virtual TestOutcome GetOutcome(DataRow row, int rowNumber)
    {
        var outcomeValue = GetCellValue(row, _excelResultParameters.OutcomeColumnName);
        return ConvertOutcome(outcomeValue, rowNumber);
    }

    protected virtual TestOutcome ConvertOutcome(string outcomeValue, int rowNumber)
    {
        if (string.IsNullOrWhiteSpace(outcomeValue))
            return TestOutcome.NotExecuted;

        var convertedOutcomeValue = _excelResultParameters.OutcomeMappings.TryGetValue(outcomeValue, out var mappedValue) ? 
                mappedValue : outcomeValue;

        if (Enum.TryParse<TestOutcome>(convertedOutcomeValue, true, out var outcome))
            return outcome;

        throw new SpecSyncException($"Invalid outcome value or the column {_excelResultParameters.OutcomeColumnName} is not defined at row {rowNumber}: '{outcomeValue}'. Possible values: {string.Join(", ", Enum.GetNames(typeof(TestOutcome)))}. You can map custom values using the 'OutcomeMapping' parameter in 'PASS=Passed,FAIL=Failed' format.");
    }

    private bool TryGetCellValue(DataRow row, string columnName, out string value)
    {
        if (row.Table.Columns.Contains(columnName))
        {
            value = row[columnName]?.ToString();
            return true;
        }

        value = null;
        return false;
    }

    private string GetCellValue(DataRow row, string columnName)
    {
        if (TryGetCellValue(row, columnName, out var value))
            return value;
        return null;
    }

    private string GetErrorMessage(DataRow row)
    {
        return GetCellValue(row, _excelResultParameters.ErrorMessageColumnName);
    }

    private string GetMethodName(DataRow row, ITagServices tagServices)
    {
        return GetTestCaseId(row, tagServices) ??
               GetCellValue(row, _excelResultParameters.ScenarioColumnName) ??
               string.Empty;
    }

    private string GetTestCaseId(DataRow row, ITagServices tagServices)
    {
        var cellValue = GetCellValue(row, _excelResultParameters.TestCaseIdColumnName);
        return GetTestCaseLink(cellValue, tagServices)?.TestCaseId.ToString();
    }

    public static TestCaseLink GetTestCaseLink(string idCellValue, ITagServices tagServices)
    {
        if (string.IsNullOrWhiteSpace(idCellValue))
            return null;

        var tags = new ILocalTestCaseTag[] { new LocalTestCaseTag(idCellValue) };
        var testCaseLink = tagServices.GetTestCaseLinkFromTags(tags);

        if (testCaseLink == null)
        {
            var testCaseId = int.Parse(idCellValue);
            testCaseLink = new TestCaseLink(TestCaseIdentifier.CreateExistingFromNumericId(testCaseId), "");
        }

        return testCaseLink;
    }

    private string GetClassName(DataRow row)
    {
        return GetCellValue(row, _excelResultParameters.FeatureFileColumnName) ??
               GetCellValue(row, _excelResultParameters.FeatureColumnName) ??
               string.Empty;
    }

    private string GetName(DataRow row, ITagServices tagServices)
    {
        return GetCellValue(row, _excelResultParameters.TestNameColumnName) ??
               GetCellValue(row, _excelResultParameters.ScenarioColumnName) ??
               GetTestCaseId(row, tagServices) ??
               string.Empty;
    }

    public DataTable LoadExcelDataTable(string filePath, string sheetName = null)
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