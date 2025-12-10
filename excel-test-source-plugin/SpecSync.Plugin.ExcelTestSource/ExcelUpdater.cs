using ClosedXML.Excel;
using SpecSync.Configuration;
using SpecSync.Parsing;

namespace SpecSync.Plugin.ExcelTestSource;

public class ExcelUpdater(
    XLWorkbook workbook,
    string filePath,
    SpecSyncConfiguration configuration,
    ExcelTestSourceParameters parameters)
    : SourceDocumentUpdaterBase
{
    private bool _isDirty = false;

    public override bool IsDirty => _isDirty;

    public override bool Flush()
    {
        if (!IsDirty)
            return false;

        workbook.SaveAs(filePath);
        _isDirty = false;
        return true;
    }

    public override void SetArtifactLink(ILocalArtifact localArtifact, IdLink testCaseLink)
    {
        _isDirty = true;
        var excelLocalTestCase = (ExcelLocalTestCase)localArtifact;

        var row = excelLocalTestCase.Worksheet.Row(excelLocalTestCase.TestCaseRowNumber);
        row.Cell(excelLocalTestCase.IdColumn).Value = XLCellValue.FromObject(GetTestCaseLinkValue(testCaseLink));
    }

    private object GetTestCaseLinkValue(IdLink testCaseLink)
    {
        if (parameters.WriteIdWithPrefix)
        {
            return GetTagName(testCaseLink);
        }

        return testCaseLink.Id.GetNumericId();
    }

    protected string GetTagName(IdLink testCaseLink)
    {
        return $"{testCaseLink.LinkPrefix}{configuration.Synchronization.TagPrefixSeparators[0]}{testCaseLink.Id}";
    }
}