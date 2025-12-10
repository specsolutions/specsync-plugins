using ClosedXML.Excel;
using SpecSync.Analyzing;
using SpecSync.Integration.AzureDevOps;
using SpecSync.Parsing;
using SpecSync.Synchronization;

namespace SpecSync.Plugin.ExcelTestSource;

public class ExcelLocalTestCase(
    string name,
    ILocalArtifactTag[] tags,
    IdLink? testCaseLink,
    TestCaseStepSyncData[] steps,
    IXLWorksheet worksheet,
    int testCaseRowNumber,
    string idColumn,
    string? description,
    string? automatedTestName)
    : ILocalTestCase, IAutomationSettingsProvider
{
    public string Name { get; } = name;
    public string? Description { get; } = description;
    public string? AutomatedTestName { get; } = automatedTestName;
    public AcceptanceCriterion? TestedRule => null;
    public ILocalArtifactTag[] Tags { get; } = tags;
    public IdLink? IdLink { get; } = testCaseLink;
    public TestCaseStepSyncData[] Steps { get; } = steps;
    public LocalTestCaseDataRow[]? DataRows => null;
    public string[]? ParameterNames => null;
    public int TestCount => 1;

    public IXLWorksheet Worksheet { get; } = worksheet;
    public int TestCaseRowNumber { get; } = testCaseRowNumber;
    public string IdColumn { get; } = idColumn;

    public AutomationSettings? GetAutomationSettings(ISpecSyncSettings settings, IArtifactSyncContext artifactSyncContext)
    {
        if (AutomatedTestName == null)
            return null;

        return new AutomationSettings(
            artifactSyncContext.SourceDocument.SourceReference.ProjectRelativePath,
            AutomatedTestName,
            settings.Configuration.Synchronization.Automation.AutomatedTestType ?? "Unknown");
    }
}