using System;
using SpecSync.Analyzing;
using SpecSync.Parsing;
using SpecSync.Synchronization;

namespace SpecSync.Plugin.OnlyPublishTestResults;

public class TestCaseResultSourceParser : ILocalTestCaseContainerParser
{
    public string ServiceDescription => "Test Case Result source parser";

    public bool CanProcess(LocalTestCaseContainerParseArgs args)
        => args.SourceFile is TestCaseResultDocumentSource;

    public ILocalTestCaseContainer Parse(LocalTestCaseContainerParseArgs args)
    {
        var source = (TestCaseResultDocumentSource)args.SourceFile;

        return new TestCaseResultDocument(args.BddProject, source, GetTestCaseLink(source.TestCaseId, args.TagServices));
    }

    private static TestCaseLink GetTestCaseLink(string idValue, ITagServices tagServices)
    {
        if (string.IsNullOrWhiteSpace(idValue))
            return null;

        var tags = new ILocalTestCaseTag[] {new LocalTestCaseTag(idValue) };
        return tagServices.GetTestCaseLinkFromTags(tags) ?? 
               new TestCaseLink(TestCaseIdentifier.CreateExisting(idValue), "");
    }
}