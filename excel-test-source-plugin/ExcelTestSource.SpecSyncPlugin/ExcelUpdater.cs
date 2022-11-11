using System;
using ClosedXML.Excel;
using SpecSync.Parsing;

namespace ExcelTestSource.SpecSyncPlugin;

public class ExcelUpdater : LocalTestCaseContainerUpdaterBase
{
    private readonly XLWorkbook _workbook;
    private bool _isDirty = false;

    public override bool IsDirty => _isDirty;

    public ExcelUpdater(XLWorkbook workbook)
    {
        _workbook = workbook;
    }

    public override bool Flush()
    {
        if (!IsDirty)
            return false;

        _workbook.Save();
        _isDirty = false;
        return true;
    }

    public override void SetTestCaseLink(ILocalTestCase localTestCase, TestCaseLink testCaseLink)
    {
        _isDirty = true;
        var excelLocalTestCase = (ExcelLocalTestCase)localTestCase;

        var row = excelLocalTestCase.Worksheet.Row(excelLocalTestCase.TestCaseRowNumber);
        row.Cell(excelLocalTestCase.IdColumn).Value = testCaseLink.TestCaseId.GetNumericId();
    }
}