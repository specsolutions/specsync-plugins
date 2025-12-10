using SpecSync.Parsing;
using SpecSync.Projects;

namespace SpecSync.Plugin.ExcelTestSource;

public class ExcelSourceDocument(
    string name,
    ISyncProject project,
    ISourceFile sourceFile,
    ILocalTestCase[] localTestCases,
    ISourceDocumentUpdater updater)
    : ISourceDocument
{
    public ISyncProject Project { get; } = project;
    public ISourceReference SourceReference { get; set; } = sourceFile;
    public string Name { get; } = name;
    public string? Description => null;
    public ILocalTestCase[] LocalTestCases { get; } = localTestCases;
    public ISourceDocumentUpdater Updater { get; } = updater;
    public IKeywordParser KeywordParser { get; } = new NoKeywordParser();

    public string? GetSource() => null;
    public string? GetLocalArtifactSource(ILocalArtifact localArtifact) => null;
    public ILocalArtifactTag[] Tags => [];
    public object? SourceObject => null;
    public ILocalRequirement[] LocalRequirements => [];
}