using SpecSync.Analyzing;

namespace SpecSync.Plugin.OnlyPublishTestResults;

public class TestCaseResultAnalyzer : ILocalArtifactAnalyzer
{
    public string ServiceDescription => "Test Case Result analyzer";

    public bool CanProcess(LocalArtifactAnalyzerArgs args)
        => args.LocalArtifact is TestCaseResultDocument;

    public ArtifactSyncData Analyze(LocalArtifactAnalyzerArgs args)
    {
        var excelLocalTestCase = (TestCaseResultDocument)args.LocalArtifact;
        return new ArtifactSyncData
        {
            Title = excelLocalTestCase.Name,
            Links = args.TagServices.GetLinkData(args.ArtifactSyncContext),
            Tags = args.TagServices.GetTagData(args.ArtifactSyncContext)
        };
    }
}