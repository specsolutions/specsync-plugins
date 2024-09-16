using System;
using System.Data;
using System.IO;
using ExcelDataReader;
using SpecSync.PublishTestResults;
using SpecSync.PublishTestResults.Loaders;
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
                MethodName = GetMethodName(row),
                Name = GetName(row),
            };

            if (IsEmptyTestDefinition(testDefinition))
            {
                args.Tracer.LogVerbose($"Row {rowNumber} does not contain test reference. Skipping.");
                continue;
            }

            var testRunTestResult = new TestRunTestResult
            {
                Name = $"Excel row {rowNumber}",
                Outcome = GetOutcome(row[_excelResultParameters.OutcomeColumnName].ToString(), rowNumber),
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

    protected virtual TestOutcome GetOutcome(string outcomeValue, int rowNumber)
    {
        if (Enum.TryParse<TestOutcome>(outcomeValue, true, out var outcome))
            return outcome;

        throw new SpecSyncException($"Invalid outcome value at row {rowNumber}: '{outcomeValue}'. Possible values: {string.Join(", ", Enum.GetNames(typeof(TestOutcome)))}.");
    }

    private string GetErrorMessage(DataRow row)
    {
        if (string.IsNullOrEmpty(_excelResultParameters.ErrorMessageColumnName) ||
            !row.Table.Columns.Contains(_excelResultParameters.ErrorMessageColumnName))
            return null;
        return row[_excelResultParameters.ErrorMessageColumnName].ToString();
    }

    private string GetMethodName(DataRow row)
    {
        if (row.Table.Columns.Contains(_excelResultParameters.TestCaseIdColumnName))
            return row[_excelResultParameters.TestCaseIdColumnName].ToString();
        if (row.Table.Columns.Contains(_excelResultParameters.ScenarioColumnName))
            return row[_excelResultParameters.ScenarioColumnName].ToString();
        return string.Empty;
    }

    private string GetClassName(DataRow row)
    {
        if (row.Table.Columns.Contains(_excelResultParameters.FeatureFileColumnName))
            return row[_excelResultParameters.FeatureFileColumnName].ToString();
        if (row.Table.Columns.Contains(_excelResultParameters.FeatureColumnName))
            return row[_excelResultParameters.FeatureColumnName].ToString();
        return string.Empty;
    }

    private string GetName(DataRow row)
    {
        string testName = null;
        if (row.Table.Columns.Contains(_excelResultParameters.TestNameColumnName))
            testName = row[_excelResultParameters.TestNameColumnName].ToString();

        if (string.IsNullOrWhiteSpace(testName) && row.Table.Columns.Contains(_excelResultParameters.ScenarioColumnName))
            testName = row[_excelResultParameters.ScenarioColumnName].ToString();

        if (string.IsNullOrWhiteSpace(testName))
            testName = GetMethodName(row);

        return testName;
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