using SpecSync.Analyzing;

namespace SpecSync.Plugin.ExcelTestSource;

public class ExcelTestCaseAnalyzer : ILocalArtifactAnalyzer
{
    public string ServiceDescription => "Excel Test Case analyzer";

    public bool CanProcess(LocalArtifactAnalyzerArgs args)
        => args.LocalArtifact is ExcelLocalTestCase;

    public ArtifactSyncData Analyze(LocalArtifactAnalyzerArgs args)
    {
        var excelLocalTestCase = (ExcelLocalTestCase)args.LocalArtifact;
        return new ArtifactSyncData
        {
            Title = excelLocalTestCase.Name,
            Links = args.TagServices.GetLinkData(args.ArtifactSyncContext),
            Tags = args.TagServices.GetTagData(args.ArtifactSyncContext),
            TestSteps = excelLocalTestCase.Steps.ToList()
        };
    }
}