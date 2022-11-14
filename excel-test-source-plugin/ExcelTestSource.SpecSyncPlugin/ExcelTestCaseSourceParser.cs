using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using SpecSync.Analyzing;
using SpecSync.Parsing;
using SpecSync.Synchronization;
using SpecSync.Tracing;

namespace ExcelTestSource.SpecSyncPlugin;

public class ExcelTestCaseSourceParser : ILocalTestCaseContainerParser
{
    public string ServiceDescription => "Excel Test Case source parser";

    public bool CanProcess(LocalTestCaseContainerParseArgs args) 
        => ".xlsx".Equals(Path.GetExtension(args.SourceFile.ProjectRelativePath), StringComparison.InvariantCultureIgnoreCase);

    public ILocalTestCaseContainer Parse(LocalTestCaseContainerParseArgs args)
    {
        const string idColumn = "A";
        const string titleColumn = "C";
        const string stepIndexColumn = "D";
        const string stepActionColumn = "E";
        const string stepExpectedValueColumn = "F";
        const string tagsColumn = "J";

        var filePath = args.BddProject.GetFullPath(args.SourceFile.ProjectRelativePath);
        var wb = OpenWorkbook(filePath, out var isReadOnly, args);

        var localTestCases = new List<ILocalTestCase>();

        foreach (var worksheet in wb.Worksheets)
        {
            var testCaseRows = worksheet.RowsUsed().Skip(1).ToArray();
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
                var tagsCellValue = row.Cell(tagsColumn).GetString();
                if (!string.IsNullOrWhiteSpace(tagsCellValue))
                {
                    tags.AddRange(tagsCellValue.Split(',').Select(t => new LocalTestCaseTag(t.Trim())));
                }

                var steps = new List<TestStepSourceData>();
                while (rowIndex < testCaseRows.Length - 1 && !testCaseRows[rowIndex + 1].Cell(stepIndexColumn).IsEmpty())
                {
                    rowIndex++;
                    var stepRow = testCaseRows[rowIndex];
                    var stepAction = stepRow.Cell(stepActionColumn).GetString();
                    if (!string.IsNullOrWhiteSpace(stepAction))
                        steps.Add(new TestStepSourceData
                        {
                            Text = new ParameterizedText(stepAction),
                            IsExpectedResult = false
                        });
                    var expectedResult = stepRow.Cell(stepExpectedValueColumn).GetString();
                    if (!string.IsNullOrWhiteSpace(expectedResult))
                        steps.Add(new TestStepSourceData
                        {
                            Text = new ParameterizedText(expectedResult),
                            IsThenStep = true
                        });
                }

                var testCase = new ExcelLocalTestCase(testCaseTitle, tags.ToArray(), testCaseLink, steps.ToArray(),
                    worksheet, row.RowNumber(), idColumn);

                localTestCases.Add(testCase);
            }
        }

        var updater = isReadOnly ? null : new ExcelUpdater(wb, filePath);

        return new ExcelTestCaseContainer(Path.GetFileNameWithoutExtension(filePath), args.BddProject, args.SourceFile, localTestCases.ToArray(), updater);
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
            args.Tracer.LogVerbose("Unable to open workbook", ex);
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