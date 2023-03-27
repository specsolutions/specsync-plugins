using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SpecSync.Parsing;
using SpecSync.Plugin.PostmanTestSource.Postman;
using SpecSync.Plugin.PostmanTestSource.Postman.Models;
using SpecSync.Projects;

namespace SpecSync.Plugin.PostmanTestSource.Projects;

[DebuggerDisplay("{ProjectRelativePath}")]
public class PostmanFolderItem : ISourceFile, IPostmanItem, ILocalTestCaseContainer
{
    private readonly Item _modelItem;
    public string Type => "Postman Folder";
    public string ProjectRelativePath { get; }
    public PostmanItemMetadata Metadata { get; }

    public PostmanTestItem[] Tests { get; }
    public PostmanFolderItem[] SubFolders { get; }

    public PostmanFolderItem(string projectRelativePath, IList<IPostmanItem> subItems, Item modelItem, PostmanItemMetadata metadata)
    {
        _modelItem = modelItem;
        ProjectRelativePath = projectRelativePath;
        Metadata = metadata;
        Tests = subItems.OfType<PostmanTestItem>().ToArray();
        SubFolders = subItems.OfType<PostmanFolderItem>().ToArray();
        Name = modelItem.Name;
    }

    public PostmanFolderItem(string projectRelativePath, IList<IPostmanItem> subItems, Collection collection, PostmanItemMetadata metadata = null)
        : this(projectRelativePath, subItems, collection.ToItem(), metadata ?? new PostmanItemMetadata())
    {
    }

    #region ILocalTestCaseContainer implementation

    public IBddProject BddProject { get; set; }
    public ISourceFile SourceFile => this;
    public string Name { get; }
    public string Description => Metadata.CleanedDocumentation ?? _modelItem.Request.Description ?? _modelItem.Description;
    // ReSharper disable once CoVariantArrayConversion
    public ILocalTestCase[] LocalTestCases => Tests;
    public ILocalTestCaseContainerUpdater Updater { get; set; }
    public IKeywordParser KeywordParser => PostmanKeywordParser.Instance;

    public string GetLocalTestCaseContainerSource() => null;
    public string GetLocalTestCaseSource(ILocalTestCase localTestCase) => null;

    #endregion
}