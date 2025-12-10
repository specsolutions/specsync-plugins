using ClosedXML.Excel;
using SpecSync.Analyzing;
using SpecSync.Parsing;
using SpecSync.Projects;
using SpecSync.Synchronization;
using SpecSync.Tracing;

namespace SpecSync.Plugin.ExcelTestSource;

public class ExcelSourceDocumentParser(ExcelTestSourceParameters parameters) : ISourceDocumentParser
{
    public string ServiceDescription => "Excel Test Case source parser";

    public bool CanProcess(SourceDocumentParserArgs args)
        => ".xlsx".Equals(Path.GetExtension(args.SourceReference.ProjectRelativePath), StringComparison.InvariantCultureIgnoreCase);

    private string? GetFieldColumn(IXLRow headerRow, string field, bool throwIfMissing)
    {
        var cell = headerRow.Cells().FirstOrDefault(c => field.Equals(c.GetString(), StringComparison.InvariantCultureIgnoreCase));
        if (cell == null && throwIfMissing)
            throw new MissingColumnException(field, headerRow.Worksheet.Name);
        return cell?.WorksheetColumn().ColumnLetter();
    }

    public ISourceDocument Parse(SourceDocumentParserArgs args)
    {
        var filePath = args.Project.GetFullPath(args.SourceReference);
        var wb = OpenWorkbook(filePath, out var isReadOnly, args);

        var localTestCases = new List<ILocalTestCase>();
        var firstWorksheet = true;

        foreach (var worksheet in wb.Worksheets)
        {
            try
            {
                ProcessWorksheet(worksheet, localTestCases, args.TagServices);
            }
            catch (MissingColumnException ex)
            {
                if (firstWorksheet)
                    throw;
                args.Tracer.TraceInformation($"Worksheet '{worksheet.Name}' is skipped because of missing column '{ex.ColumnName}'");
            }

            firstWorksheet = false;
        }

        ISourceDocumentUpdater updater = isReadOnly ? new ReadOnlySourceDocumentUpdater() : new ExcelUpdater(wb, filePath, args.Configuration, parameters);

        return new ExcelSourceDocument(Path.GetFileNameWithoutExtension(filePath), args.Project, args.SourceReference.AsSourceFile(), localTestCases.ToArray(), updater);
    }

    private void ProcessWorksheet(IXLWorksheet worksheet, List<ILocalTestCase> localTestCases, ITagServices tagServices)
    {
        var headerRow = worksheet.RowsUsed().FirstOrDefault();
        if (headerRow == null)
            return;
        var testCaseRows = worksheet.RowsUsed().Skip(1).ToArray();
        if (testCaseRows.Length == 0)
            return;

        var idColumn = GetFieldColumn(headerRow, parameters.TestCaseIdColumnName, true)!;
        var titleColumn = GetFieldColumn(headerRow, parameters.TitleColumnName, true)!;
        var stepActionColumn = GetFieldColumn(headerRow, parameters.TestStepActionColumnName, true)!;
        var stepIndexColumn = GetFieldColumn(headerRow, parameters.TestStepColumnName, false);
        var stepExpectedValueColumn = GetFieldColumn(headerRow, parameters.TestStepExpectedColumnName, false);
        var stepIndicatorColumns = new[] { stepIndexColumn, stepActionColumn, stepExpectedValueColumn }.Where(c => c != null).ToArray();
        var tagsColumn = GetFieldColumn(headerRow, parameters.TagsColumnName, false);
        var descriptionColumn = GetFieldColumn(headerRow, parameters.DescriptionColumnName, false);
        var automationStatusColumn = GetFieldColumn(headerRow, parameters.AutomationStatusColumnName, false);
        var automatedTestNameColumn = GetFieldColumn(headerRow, parameters.AutomatedTestNameColumnName, false);

        for (int rowIndex = 0; rowIndex < testCaseRows.Length; rowIndex++)
        {
            var row = testCaseRows[rowIndex];
            var testCaseTitle = row.Cell(titleColumn).GetString();
            if (string.IsNullOrWhiteSpace(testCaseTitle))
                continue;

            var idCellValue = row.Cell(idColumn).GetString();
            var testCaseLink = GetTestCaseLink(idCellValue, tagServices);

            var tags = new List<ILocalArtifactTag>();
            if (tagsColumn != null)
            {
                var tagsCellValue = row.Cell(tagsColumn).GetString();
                if (!string.IsNullOrWhiteSpace(tagsCellValue))
                {
                    tags.AddRange(tagsCellValue.Split(',').Select(t => new LocalArtifactTag(t.Trim())));
                }
            }

            bool HasStepData(IXLRow r) => !stepIndicatorColumns.All(c => r.Cell(c).IsEmpty());
            var steps = new List<TestCaseStepSyncData>();
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
                    steps.Add(new TestCaseStepSyncData
                    {
                        Text = new ParameterizedText(stepAction),
                        IsOutcomeStep = false
                    });
                if (stepExpectedValueColumn != null)
                {
                    var expectedResult = stepRow.Cell(stepExpectedValueColumn).GetString();
                    if (!string.IsNullOrWhiteSpace(expectedResult))
                        steps.Add(new TestCaseStepSyncData
                        {
                            Text = new ParameterizedText(expectedResult),
                            IsOutcomeStep = true
                        });
                }
            }

            string? description = null;
            if (descriptionColumn != null)
            {
                description = row.Cell(descriptionColumn).GetString();
            }

            if (automationStatusColumn != null)
            {
                var automationStatus = row.Cell(automationStatusColumn).GetString();
                if (!"automated".Equals(automationStatus, StringComparison.InvariantCultureIgnoreCase))
                    tags.Add(new LocalArtifactTag(ExcelTestSourcePlugin.ManualTagName));
            }

            string? automatedTestName = null;
            if (automatedTestNameColumn != null)
            {
                automatedTestName = row.Cell(automatedTestNameColumn).GetString();
            }

            foreach (var fieldUpdaterColumnParameter in parameters.FieldUpdateColumns)
            {
                var column = GetFieldColumn(headerRow, fieldUpdaterColumnParameter.ColumnName, false);
                if (column != null)
                {
                    var value = row.Cell(column).GetString() ?? "";
                    tags.Add(new LocalArtifactTag($"{fieldUpdaterColumnParameter.TagNamePrefix ?? fieldUpdaterColumnParameter.GeneratedTagNamePrefix}{value}"));
                }
            }

            var testCase = new ExcelLocalTestCase(testCaseTitle, tags.ToArray(), testCaseLink, steps.ToArray(),
                worksheet, row.RowNumber(), idColumn, description, automatedTestName);

            localTestCases.Add(testCase);
        }
    }

    private static IdLink? GetTestCaseLink(string idCellValue, ITagServices tagServices)
    {
        if (string.IsNullOrWhiteSpace(idCellValue))
            return null;

        var tags = new ILocalArtifactTag[] {new LocalArtifactTag(idCellValue) };
        var testCaseLink = tagServices.GetTestCaseLinkFromTags(tags);

        if (testCaseLink == null)
        {
            var testCaseId = int.Parse(idCellValue);
            testCaseLink = new IdLink(TestCaseIdentifier.CreateExistingFromNumericId(testCaseId), "");
        }

        return testCaseLink;
    }

    private XLWorkbook OpenWorkbook(string filePath, out bool isReadOnly, SourceDocumentParserArgs args)
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