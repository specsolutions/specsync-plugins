using SpecSync.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using SpecSync.Plugin.PostmanTestSource.Postman;
using SpecSync.Plugin.PostmanTestSource.Postman.Models;
using SpecSync.Plugin.PostmanTestSource.Projects;

namespace SpecSync.Plugin.PostmanTestSource;

public class PostmanCollectionLoader : IBddProjectLoader
{
    public bool CanProcess(BddProjectLoaderArgs args) => true;

    public string ServiceDescription => "Postman Collection Loader";

    private bool IsTestItem(Item item, List<IPostmanItem> subPostmanItems)
    {
        return item.Request != null;
    }

    private IPostmanItem ProcessItem(Item item, List<PostmanFolderCollection> folderCollections, string rootName)
    {
        var itemPath = $"{rootName} / {item.Name}";
        var subPostmanItems = new List<IPostmanItem>();
        if (item.Items != null)
            foreach (var subItem in item.Items)
            {
                subPostmanItems.Add(ProcessItem(subItem, folderCollections, itemPath));
            }

        var isTestItem = IsTestItem(item, subPostmanItems);
        if (isTestItem)
            return new PostmanTestItem();
        var folderCollection = new PostmanFolderCollection(itemPath, subPostmanItems);
        folderCollections.Add(folderCollection);
        return folderCollection;
    }

    private void CreateRootItem(Collection collection, List<PostmanFolderCollection> folderCollections)
    {
        var rootName = collection.Info.Name;
        var testTree = new PostmanFolderCollection(rootName, 
            collection.Items.Select(i => ProcessItem(i, folderCollections, rootName)).ToList());
        folderCollections.Insert(0, testTree);
    }

    public IBddProject LoadProject(BddProjectLoaderArgs args)
    {
        var api = new PostmanApi(PostmanApiConnection.Create(args.Tracer));

        var collectionId = "2c49b8c3-0f1a-43f5-8a18-5d7f6c50c0ac";
        var collection = api.GetCollection(collectionId).Collection;

        var folderCollections = new List<PostmanFolderCollection>();

        CreateRootItem(collection, folderCollections);

        return new PostmanProject(folderCollections, args.BaseFolder);
    }

    public string GetSourceDescription(BddProjectLoaderArgs args) 
        => "Postman Collection";
}