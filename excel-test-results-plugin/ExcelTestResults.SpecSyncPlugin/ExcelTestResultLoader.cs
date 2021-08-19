using System;
using System.Data;
using System.IO;
using ExcelDataReader;
using SpecSync.AzureDevOps.PublishTestResults;
using SpecSync.AzureDevOps.PublishTestResults.Loaders;
using SpecSync.AzureDevOps.Utils;

namespace ExcelTestResults.SpecSyncPlugin
{
    public class ExcelTestResultLoader : ITestResultLoader
    {
        public const string FormatSpecifier = "Excel";
        private readonly ExcelResultSpecification _excelResultSpecification;

        public ExcelTestResultLoader(ExcelResultSpecification excelResultSpecification)
        {
            _excelResultSpecification = excelResultSpecification;
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
            _excelResultSpecification.Verify();

            var testResultTable = LoadExcelDataTable(args.TestResultFilePath, _excelResultSpecification.TestResultSheetName);

            var localTestRun = new LocalTestRun
            {
                TestFrameworkIdentifier = FormatSpecifier,
                Name = $"{Path.GetFileName(args.TestResultFilePath)} - {testResultTable.TableName}"
            };

            foreach (DataRow row in testResultTable.Rows)
            {
                var testDefinition = new TestRunTestDefinition
                {
                    ClassName = GetClassName(row),
                    MethodName = GetMethodName(row),
                    Name = GetName(row),
                };
                var testRunTestResult = new TestRunTestResult
                {
                    Outcome = GetOutcome(row[_excelResultSpecification.OutcomeColumnName].ToString()),
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

        protected virtual TestOutcome GetOutcome(string outcomeValue)
        {
            if (Enum.TryParse<TestOutcome>(outcomeValue, true, out var outcome))
                return outcome;

            throw new SpecSyncException($"Invalid outcome value: '{outcome}'. Possible values: {string.Join(", ", Enum.GetNames(typeof(TestOutcome)))}.");
        }

        private string GetErrorMessage(DataRow row)
        {
            if (string.IsNullOrEmpty(_excelResultSpecification.ErrorMessageColumnName))
                return null;
            return row[_excelResultSpecification.ErrorMessageColumnName].ToString();
        }

        private string GetMethodName(DataRow row)
        {
            if (_excelResultSpecification.MatchByScenario)
                return row[_excelResultSpecification.ScenarioColumnName].ToString();
            if (_excelResultSpecification.MatchByTestCaseId)
                return row[_excelResultSpecification.TestCaseIdColumnName].ToString();
            return string.Empty;
        }

        private string GetClassName(DataRow row)
        {
            if (_excelResultSpecification.MatchByFeatureFile)
                return row[_excelResultSpecification.FeatureFileColumnName].ToString();
            if (_excelResultSpecification.MatchByFeature)
                return row[_excelResultSpecification.FeatureColumnName].ToString();
            return string.Empty;
        }

        private string GetName(DataRow row)
        {
            if (!string.IsNullOrEmpty(_excelResultSpecification.TestNameColumnName))
                return row[_excelResultSpecification.TestNameColumnName].ToString();
            return row[0].ToString();
        }

        public DataTable LoadExcelDataTable(string filePath, string sheetName = null)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
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
}