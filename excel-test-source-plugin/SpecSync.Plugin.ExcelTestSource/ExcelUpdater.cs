using System;
using ClosedXML.Excel;
using SpecSync.Configuration;
using SpecSync.Parsing;

namespace SpecSync.Plugin.ExcelTestSource;

public class ExcelUpdater : LocalTestCaseContainerUpdaterBase
{
    private readonly XLWorkbook _workbook;
    private readonly string _filePath;
    private readonly SpecSyncConfiguration _configuration;
    private readonly ExcelTestSourceParameters _parameters;
    private bool _isDirty = false;

    public override bool IsDirty => _isDirty;

    public ExcelUpdater(XLWorkbook workbook, string filePath, SpecSyncConfiguration configuration, ExcelTestSourceParameters parameters)
    {
        _workbook = workbook;
        _filePath = filePath;
        _configuration = configuration;
        _parameters = parameters;
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
        row.Cell(excelLocalTestCase.IdColumn).Value = GetTestCaseLinkValue(testCaseLink);
    }

    private object GetTestCaseLinkValue(TestCaseLink testCaseLink)
    {
        if (_parameters.WriteIdWithPrefix)
        {
            return GetTagName(testCaseLink);
        }

        return testCaseLink.TestCaseId.GetNumericId();
    }

    protected string GetTagName(TestCaseLink testCaseLink)
    {
        return $"{testCaseLink.LinkPrefix}{_configuration.Synchronization.TagPrefixSeparators[0]}{testCaseLink.TestCaseId}";
    }
}