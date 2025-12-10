using SpecSync.Analyzing;
using SpecSync.Parsing;
using SpecSync.Synchronization;

namespace SpecSync.Plugin.OnlyPublishTestResults;

public class TestCaseResultSourceParser : ISourceDocumentParser
{
    public string ServiceDescription => "Test Case Result source parser";

    public bool CanProcess(SourceDocumentParserArgs args)
        => args.SourceReference is TestCaseResultDocumentSource;

    public ISourceDocument Parse(SourceDocumentParserArgs args)
    {
        var source = (TestCaseResultDocumentSource)args.SourceReference;

        return new TestCaseResultDocument(args.Project, source, GetTestCaseLink(source.TestCaseId, args.TagServices));
    }

    private static IdLink? GetTestCaseLink(string idValue, ITagServices tagServices)
    {
        if (string.IsNullOrWhiteSpace(idValue))
            return null;

        var tags = new ILocalArtifactTag[] {new LocalArtifactTag(idValue) };
        return tagServices.GetTestCaseLinkFromTags(tags) ?? 
               new IdLink(TestCaseIdentifier.CreateExisting(idValue), "");
    }
}