using System;
using ClosedXML.Excel;
using SpecSync.Parsing;

namespace SpecSync.Plugin.PostmanTestSource;

public class ExcelUpdater : LocalTestCaseContainerUpdaterBase
{
    private readonly XLWorkbook _workbook;
    private readonly string _filePath;
    private bool _isDirty = false;

    public override bool IsDirty => _isDirty;

    public ExcelUpdater(XLWorkbook workbook, string filePath)
    {
        _workbook = workbook;
        _filePath = filePath;
    }

    public override bool Flush()
    {
        if (!IsDirty)
            return false;

        _workbook.SaveAs(_filePath);
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