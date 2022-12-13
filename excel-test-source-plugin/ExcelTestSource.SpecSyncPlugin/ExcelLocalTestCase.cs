using ClosedXML.Excel;
using SpecSync.Analyzing;
using SpecSync.Parsing;

namespace ExcelTestSource.SpecSyncPlugin;

public class ExcelLocalTestCase : ILocalTestCase
{
    public string Name { get; }
    public string Description { get; }
    public string TestedRule => null;
    public ILocalTestCaseTag[] Tags { get; }
    public TestCaseLink TestCaseLink { get; }
    public TestStepSourceData[] Steps { get; }
    public bool IsDataDrivenTest => false;
    public LocalTestCaseDataRow[] DataRows => null;
    public int TestCount => 1;

    public IXLWorksheet Worksheet { get; }
    public int TestCaseRowNumber { get; }
    public string IdColumn { get; }

    public ExcelLocalTestCase(string name, ILocalTestCaseTag[] tags, TestCaseLink testCaseLink, TestStepSourceData[] steps, IXLWorksheet worksheet, int testCaseRowNumber, string idColumn, string description)
    {
        Name = name;
        Tags = tags;
        TestCaseLink = testCaseLink;
        Steps = steps;
        Worksheet = worksheet;
        TestCaseRowNumber = testCaseRowNumber;
        IdColumn = idColumn;
        Description = description;
    }
}