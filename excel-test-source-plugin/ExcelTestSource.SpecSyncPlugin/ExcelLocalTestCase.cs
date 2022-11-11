using SpecSync.Analyzing;
using SpecSync.Parsing;

namespace ExcelTestSource.SpecSyncPlugin;

public class ExcelLocalTestCase : ILocalTestCase
{
    public string Name { get; }
    public string Description => null;
    public string TestedRule => null;
    public ILocalTestCaseTag[] Tags { get; }
    public TestCaseLink TestCaseLink { get; }
    public TestStepSourceData[] Steps { get; }
    public bool IsDataDrivenTest => false;
    public LocalTestCaseDataRow[] DataRows => null;
    public int TestCount => 1;

    public ExcelLocalTestCase(string name, ILocalTestCaseTag[] tags, TestCaseLink testCaseLink, TestStepSourceData[] steps)
    {
        Name = name;
        Tags = tags;
        TestCaseLink = testCaseLink;
        Steps = steps;
    }
}