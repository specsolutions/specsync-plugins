using ClosedXML.Excel;
using SpecSync;
using SpecSync.Analyzing;
using SpecSync.Integration.AzureDevOps;
using SpecSync.Parsing;
using SpecSync.Synchronization;

namespace SpecSync.Plugin.ExcelTestSource;

public class ExcelLocalTestCase : ILocalTestCase, IAutomationSettingsProvider
{
    public string Name { get; }
    public string Description { get; }
    public string AutomatedTestName { get; }
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

    public ExcelLocalTestCase(string name, ILocalTestCaseTag[] tags, TestCaseLink testCaseLink,
        TestStepSourceData[] steps, IXLWorksheet worksheet, int testCaseRowNumber, string idColumn, string description,
        string automatedTestName)
    {
        Name = name;
        Tags = tags;
        TestCaseLink = testCaseLink;
        Steps = steps;
        Worksheet = worksheet;
        TestCaseRowNumber = testCaseRowNumber;
        IdColumn = idColumn;
        Description = description;
        AutomatedTestName = automatedTestName;
    }

    public AutomationSettings GetAutomationSettings(ISyncSettings settings, ITestCaseSyncContext testCaseSyncContext)
    {
        if (AutomatedTestName == null)
            return null;

        return new AutomationSettings(
            testCaseSyncContext.LocalTestCaseContainer.SourceFile.ProjectRelativePath,
            AutomatedTestName,
            settings.Configuration.Synchronization.Automation.AutomatedTestType ?? "Unknown");
    }
}