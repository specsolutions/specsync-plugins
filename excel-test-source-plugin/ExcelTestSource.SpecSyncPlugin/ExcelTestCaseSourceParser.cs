using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using SpecSync.Analyzing;
using SpecSync.Parsing;
using SpecSync.Synchronization;

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
        XLWorkbook wb = new XLWorkbook(filePath);

        var localTestCases = new List<ILocalTestCase>();

        foreach (var worksheet in wb.Worksheets)
        {
            var testCaseRows = worksheet.RowsUsed().Skip(1).ToArray();
            for (int rowIndex = 0; rowIndex < testCaseRows.Length; rowIndex++)
            {
                var row = testCaseRows[rowIndex];
                var idCellValue = row.Cell(idColumn).GetString();
                if (string.IsNullOrWhiteSpace(idCellValue))
                    continue;
                args.Tracer.TraceInformation(idCellValue);
                var testCaseId = int.Parse(idCellValue);
                var testCaseTitle = row.Cell(titleColumn).GetString();
                var testCaseLink = new TestCaseLink(TestCaseIdentifier.CreateExistingFromNumericId(testCaseId), "");

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

                var testCase = new ExcelLocalTestCase(testCaseTitle, tags.ToArray(), testCaseLink, steps.ToArray());

                localTestCases.Add(testCase);
            }
        }

        return new ExcelTestCaseContainer(Path.GetFileNameWithoutExtension(filePath), args.BddProject, args.SourceFile, localTestCases.ToArray());
    }
}