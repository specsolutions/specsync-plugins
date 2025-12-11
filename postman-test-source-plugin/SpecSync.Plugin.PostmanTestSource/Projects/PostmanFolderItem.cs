using System.Diagnostics;
using SpecSync.Parsing;
using SpecSync.Plugin.PostmanTestSource.Postman.Models;
using SpecSync.Projects;

namespace SpecSync.Plugin.PostmanTestSource.Projects;

[DebuggerDisplay("{ProjectRelativePath}")]
public class PostmanFolderItem(
    string projectRelativePath,
    IList<IPostmanItem> subItems,
    Item modelItem,
    PostmanItemMetadata metadata)
    : ISourceFile, IPostmanItem, ISourceDocument
{
    public string Type => "Postman Folder";
    public string ProjectRelativePath { get; } = projectRelativePath;
    public PostmanItemMetadata Metadata { get; } = metadata;

    public PostmanTestItem[] Tests { get; } = subItems.OfType<PostmanTestItem>().ToArray();
    public PostmanFolderItem[] SubFolders { get; } = subItems.OfType<PostmanFolderItem>().ToArray();

    #region ISourceDocument implementation

    public ISyncProject Project { get; internal set; } = null!;
    public ISourceReference SourceReference => this;
    public string Name { get; } = modelItem.Name;
    public string Description => Metadata.CleanedDocumentation;
    // ReSharper disable once CoVariantArrayConversion
    public ILocalTestCase[] LocalTestCases => Tests;
    public ISourceDocumentUpdater Updater { get; internal set; } = null!;
    public IKeywordParser KeywordParser => PostmanKeywordParser.Instance;
    public ILocalArtifactTag[] Tags => [];
    public object? SourceObject => null;
    public ILocalRequirement[] LocalRequirements => [];

    public string? GetSource() => null;
    public string? GetLocalArtifactSource(ILocalArtifact localArtifact) => null;

    #endregion
}