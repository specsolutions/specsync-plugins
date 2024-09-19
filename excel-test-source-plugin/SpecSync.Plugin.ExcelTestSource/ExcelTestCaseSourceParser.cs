using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using SpecSync.Analyzing;
using SpecSync.Parsing;
using SpecSync.Synchronization;
using SpecSync.Tracing;
using SpecSync.Utils;

namespace SpecSync.Plugin.ExcelTestSource;

public class ExcelTestCaseSourceParser : ILocalTestCaseContainerParser
{
    private readonly ExcelTestSourceParameters _parameters;

    public ExcelTestCaseSourceParser(ExcelTestSourceParameters parameters)
    {
        _parameters = parameters;
    }

    public string ServiceDescription => "Excel Test Case source parser";

    public bool CanProcess(LocalTestCaseContainerParseArgs args)
        => ".xlsx".Equals(Path.GetExtension(args.SourceFile.ProjectRelativePath), StringComparison.InvariantCultureIgnoreCase);

    private string GetFieldColumn(IXLRow headerRow, string field, bool throwIfMissing)
    {
        var cell = headerRow.Cells().FirstOrDefault(c => field.Equals(c.GetString(), StringComparison.InvariantCultureIgnoreCase));
        if (cell == null && throwIfMissing)
            throw new MissingColumnException(field, headerRow.Worksheet.Name);
        return cell?.WorksheetColumn().ColumnLetter();
    }

    public ILocalTestCaseContainer Parse(LocalTestCaseContainerParseArgs args)
    {
        var filePath = args.BddProject.GetFullPath(args.SourceFile.ProjectRelativePath);
        var wb = OpenWorkbook(filePath, out var isReadOnly, args);

        var localTestCases = new List<ILocalTestCase>();
        var firstWorksheet = true;

        foreach (var worksheet in wb.Worksheets)
        {
            try
            {
                ProcessWorksheet(worksheet, localTestCases);
            }
            catch (MissingColumnException ex)
            {
                if (firstWorksheet)
                    throw;
                args.Tracer.TraceInformation($"Worksheet '{worksheet.Name}' is skipped because of missing column '{ex.ColumnName}'");
            }

            firstWorksheet = false;
        }

        var updater = isReadOnly ? null : new ExcelUpdater(wb, filePath);

        return new ExcelTestCaseContainer(Path.GetFileNameWithoutExtension(filePath), args.BddProject, args.SourceFile, localTestCases.ToArray(), updater);
    }

    private void ProcessWorksheet(IXLWorksheet worksheet, List<ILocalTestCase> localTestCases)
    {
        var headerRow = worksheet.RowsUsed().FirstOrDefault();
        if (headerRow == null)
            return;
        var testCaseRows = worksheet.RowsUsed().Skip(1).ToArray();
        if (testCaseRows.Length == 0)
            return;

        var idColumn = GetFieldColumn(headerRow, _parameters.TestCaseIdColumnName, true);
        var titleColumn = GetFieldColumn(headerRow, _parameters.TitleColumnName, true);
        var stepActionColumn = GetFieldColumn(headerRow, _parameters.TestStepActionColumnName, true);
        var stepIndexColumn = GetFieldColumn(headerRow, _parameters.TestStepColumnName, false);
        var stepExpectedValueColumn = GetFieldColumn(headerRow, _parameters.TestStepExpectedColumnName, false);
        var stepIndicatorColumns = new[] { stepIndexColumn, stepActionColumn, stepExpectedValueColumn }.Where(c => c != null).ToArray();
        var tagsColumn = GetFieldColumn(headerRow, _parameters.TagsColumnName, false);
        var descriptionColumn = GetFieldColumn(headerRow, _parameters.DescriptionColumnName, false);
        var automationStatusColumn = GetFieldColumn(headerRow, _parameters.AutomationStatusColumnName, false);
        var automatedTestNameColumn = GetFieldColumn(headerRow, _parameters.AutomatedTestNameColumnName, false);

        for (int rowIndex = 0; rowIndex < testCaseRows.Length; rowIndex++)
        {
            var row = testCaseRows[rowIndex];
            var testCaseTitle = row.Cell(titleColumn).GetString();
            if (string.IsNullOrWhiteSpace(testCaseTitle))
                continue;

            var idCellValue = row.Cell(idColumn).GetString();
            TestCaseLink testCaseLink = null;
            if (!string.IsNullOrWhiteSpace(idCellValue))
            {
                var testCaseId = int.Parse(idCellValue);
                testCaseLink = new TestCaseLink(TestCaseIdentifier.CreateExistingFromNumericId(testCaseId), "");
            }

            var tags = new List<ILocalTestCaseTag>();
            if (tagsColumn != null)
            {
                var tagsCellValue = row.Cell(tagsColumn).GetString();
                if (!string.IsNullOrWhiteSpace(tagsCellValue))
                {
                    tags.AddRange(tagsCellValue.Split(',').Select(t => new LocalTestCaseTag(t.Trim())));
                }
            }

            bool HasStepData(IXLRow r) => !stepIndicatorColumns.All(c => r.Cell(c).IsEmpty());
            var steps = new List<TestStepSourceData>();
            var readFirstStepFromTestCaseRow = HasStepData(row);
            while (readFirstStepFromTestCaseRow || 
                   (rowIndex < testCaseRows.Length - 1 && 
                    testCaseRows[rowIndex + 1].Cell(titleColumn).IsEmpty() && // not a test case row
                    HasStepData(testCaseRows[rowIndex + 1]))) // has step data
            {
                if (readFirstStepFromTestCaseRow)
                {
                    // keep rowIndex
                    readFirstStepFromTestCaseRow = false;
                }
                else
                {
                    rowIndex++;
                }
                var stepRow = testCaseRows[rowIndex];
                var stepAction = stepRow.Cell(stepActionColumn).GetString();
                if (!string.IsNullOrWhiteSpace(stepAction))
                    steps.Add(new TestStepSourceData
                    {
                        Text = new ParameterizedText(stepAction),
                        IsExpectedResult = false
                    });
                if (stepExpectedValueColumn != null)
                {
                    var expectedResult = stepRow.Cell(stepExpectedValueColumn).GetString();
                    if (!string.IsNullOrWhiteSpace(expectedResult))
                        steps.Add(new TestStepSourceData
                        {
                            Text = new ParameterizedText(expectedResult),
                            IsThenStep = true
                        });
                }
            }

            string description = null;
            if (descriptionColumn != null)
            {
                description = row.Cell(descriptionColumn).GetString();
            }

            if (automationStatusColumn != null)
            {
                var automationStatus = row.Cell(automationStatusColumn).GetString();
                if (!"automated".Equals(automationStatus, StringComparison.InvariantCultureIgnoreCase))
                    tags.Add(new LocalTestCaseTag(ExcelTestSourcePlugin.ManualTagName));
            }

            string automatedTestName = null;
            if (automatedTestNameColumn != null)
            {
                automatedTestName = row.Cell(automatedTestNameColumn).GetString();
            }

            foreach (var fieldUpdaterColumnParameter in _parameters.FieldUpdaterColumnParameters)
            {
                var column = GetFieldColumn(headerRow, fieldUpdaterColumnParameter.ColumnName, false);
                if (column != null)
                {
                    var value = row.Cell(column).GetString() ?? "";
                    tags.Add(new LocalTestCaseTag($"{fieldUpdaterColumnParameter.TagNamePrefix ?? fieldUpdaterColumnParameter.GeneratedTagNamePrefix}{value}"));
                }
            }

            var testCase = new ExcelLocalTestCase(testCaseTitle, tags.ToArray(), testCaseLink, steps.ToArray(),
                worksheet, row.RowNumber(), idColumn, description, automatedTestName);

            localTestCases.Add(testCase);
        }
    }

    private XLWorkbook OpenWorkbook(string filePath, out bool isReadOnly, LocalTestCaseContainerParseArgs args)
    {
        isReadOnly = false;
        try
        {
            return new XLWorkbook(filePath);
        }
        catch (IOException ex)
        {
            // The file might be open in Excel. We load it to the memory and open from there.
            // Alternatively we could copy it to a temp file and open it from there.
            args.Tracer.LogVerbose($"Unable to open file for writing. It might be open in Excel. Error: {ex.Message}");
            try
            {
                void CopyStream(Stream input, Stream output)
                {
                    byte[] buffer = new byte[8192];
                    int count;
                    while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
                        output.Write(buffer, 0, count);
                    output.Flush();
                }
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var memoryStream = new MemoryStream(); // we do not dispose the MemoryStream, as it will be used by the workbook
                CopyStream(fileStream, memoryStream);
                memoryStream.Seek(0L, SeekOrigin.Begin);
                isReadOnly = true;
                var workbook = new XLWorkbook(memoryStream);

                if (!args.Configuration.Synchronization.DisableLocalChanges)
                    args.Tracer.TraceWarning(new TraceWarningItem("Unable to open file for writing. It might be open in Excel. Linking new test cases is disabled for this file!"));

                return workbook;
            }
            catch (IOException readOnlyOpenEx)
            {
                // the alternative approach failed, let's throw the original error
                args.Tracer.LogVerbose("Opening for read-only failed.", readOnlyOpenEx);
            }

            throw;
        }
    }
}