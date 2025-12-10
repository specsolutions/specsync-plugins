using SpecSync.Parsing;
using SpecSync.Projects;

namespace SpecSync.Plugin.OnlyPublishTestResults;

public class TestCaseResultDocument(ISyncProject project, TestCaseResultDocumentSource source, IdLink? testCaseLink)
    : ISourceDocument, ILocalTestCase
{
    #region ILocalTestCaseContainer
    public ISyncProject Project { get; } = project;
    public ISourceReference SourceReference => source;
    public string Name => $"#{source.TestCaseId}";
    public string? Description => null;
    public ILocalTestCase[] LocalTestCases => [this];
    public ISourceDocumentUpdater Updater => new ReadOnlySourceDocumentUpdater();
    public IKeywordParser KeywordParser => NoKeywordParser.Instance;
    public ILocalRequirement[] LocalRequirements => [];
    public string? GetSource() => null;
    public string? GetLocalArtifactSource(ILocalArtifact localArtifact) => null;

    #endregion

    #region ILocalTestCase

    public AcceptanceCriterion? TestedRule => null;
    public ILocalArtifactTag[] Tags => [];
    public IdLink? IdLink { get; } = testCaseLink;
    public LocalTestCaseDataRow[]? DataRows => null;
    public string[]? ParameterNames => null;
    public int TestCount => 1;
    public object? SourceObject => null;

    #endregion
}