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

    public PostmanTestItem[] Tests { get; }
    public PostmanFolderItem[] SubCollections { get; }

    public PostmanFolderItem(string projectRelativePath, IList<IPostmanItem> subItems, Item modelItem)
    {
        _modelItem = modelItem;
        ProjectRelativePath = projectRelativePath;
        Tests = subItems.OfType<PostmanTestItem>().ToArray();
        SubCollections = subItems.OfType<PostmanFolderItem>().ToArray();
        Name = modelItem.Name;
        Description = modelItem.Description;
    }

    public PostmanFolderItem(string projectRelativePath, IList<IPostmanItem> subItems, Collection collection)
        : this(projectRelativePath, subItems, collection.ToItem())
    {
    }

    #region ILocalTestCaseContainer implementation

    public IBddProject BddProject { get; set; }
    public ISourceFile SourceFile => this;
    public string Name { get; }
    public string Description { get; }
    // ReSharper disable once CoVariantArrayConversion
    public ILocalTestCase[] LocalTestCases => Tests;
    public ILocalTestCaseContainerUpdater Updater { get; set; }
    public IKeywordParser KeywordParser => PostmanKeywordParser.Instance;

    public string GetLocalTestCaseContainerSource() => null;
    public string GetLocalTestCaseSource(ILocalTestCase localTestCase) => null;

    #endregion
}