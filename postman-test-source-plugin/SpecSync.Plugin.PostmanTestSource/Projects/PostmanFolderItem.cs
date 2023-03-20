using System.Collections.Generic;
using System.Linq;
using SpecSync.Parsing;
using SpecSync.Plugin.PostmanTestSource.Postman.Models;
using SpecSync.Projects;

namespace SpecSync.Plugin.PostmanTestSource.Projects;

public class PostmanFolderItem : ISourceFile, IPostmanItem, ILocalTestCaseContainer
{
    private readonly IHasItems _modelItem;
    public string Type => "Postman Collection";
    public string ProjectRelativePath { get; }

    public PostmanTestItem[] Tests { get; }
    public PostmanFolderItem[] SubCollections { get; }

    private PostmanFolderItem(string projectRelativePath, IList<IPostmanItem> subItems, IHasItems modelItem)
    {
        _modelItem = modelItem;
        ProjectRelativePath = projectRelativePath;
        Tests = subItems.OfType<PostmanTestItem>().ToArray();
        SubCollections = subItems.OfType<PostmanFolderItem>().ToArray();
    }

    public PostmanFolderItem(string projectRelativePath, IList<IPostmanItem> subItems, Item modelItem)
        : this(projectRelativePath, subItems, (IHasItems)modelItem)
    {
        Name = modelItem.Name;
        Description = modelItem.Description;
    }

    public PostmanFolderItem(string projectRelativePath, IList<IPostmanItem> subItems, Collection collection)
        : this(projectRelativePath, subItems, (IHasItems)collection)
    {
        Name = collection.Info.Name;
        Description = collection.Info.Description;
    }

    #region ILocalTestCaseContainer implementation

    public IBddProject BddProject { get; set; }
    public ISourceFile SourceFile => this;
    public string Name { get; }
    public string Description { get; }
    // ReSharper disable once CoVariantArrayConversion
    public ILocalTestCase[] LocalTestCases => Tests;
    public ILocalTestCaseContainerUpdater Updater => null;
    public IKeywordParser KeywordParser => new NoKeywordParser();

    public string GetLocalTestCaseContainerSource() => null;
    public string GetLocalTestCaseSource(ILocalTestCase localTestCase) => null;

    #endregion
}