using SpecSync.Projects;
using System.Collections.Generic;
using System.Linq;
using SpecSync.Integration.RestApiServices;
using SpecSync.Plugin.PostmanTestSource.Postman;
using SpecSync.Plugin.PostmanTestSource.Postman.Models;
using SpecSync.Plugin.PostmanTestSource.Projects;
using SpecSync.Utils;

namespace SpecSync.Plugin.PostmanTestSource;

public class PostmanCollectionLoader : IBddProjectLoader
{
    private readonly PostmanTestSourcePlugin.Parameters _parameters;
    private readonly PostmanMetadataParser _postmanMetadataParser;
    public bool CanProcess(BddProjectLoaderArgs args) => true;

    public string ServiceDescription => "Postman Collection Loader";

    public PostmanCollectionLoader(PostmanTestSourcePlugin.Parameters parameters, PostmanMetadataParser postmanMetadataParser = null)
    {
        _parameters = parameters;
        _postmanMetadataParser = postmanMetadataParser ?? new PostmanMetadataParser();
    }

    private bool IsTestItem(Item item, PostmanItemMetadata metadata, BddProjectLoaderArgs args)
    {
        return item.Request != null || PostmanFolderItemParser.GetTestCaseLinkFromMetadata(metadata, args.Configuration) != null;
    }

    private IPostmanItem ProcessItem(Item item, List<PostmanFolderItem> folderItems, string rootName, BddProjectLoaderArgs args)
    {
        var itemPath = $"{rootName}/{item.Name}";
        var metadata = _postmanMetadataParser.ParseMetadata(item);

        var isTestItem = IsTestItem(item, metadata, args);
        if (isTestItem)
            return new PostmanTestItem(item, metadata);

        var subPostmanItems = new List<IPostmanItem>();
        if (item.Items != null)
            foreach (var subItem in item.Items)
            {
                subPostmanItems.Add(ProcessItem(subItem, folderItems, itemPath, args));
            }

        var folderItem = new PostmanFolderItem(itemPath, subPostmanItems, item);
        folderItems.Add(folderItem);
        return folderItem;
    }

    private void CreateRootItem(Collection collection, List<PostmanFolderItem> folderItems, BddProjectLoaderArgs args)
    {
        var rootName = collection.Info?.Name ?? "Collection";
        var testTree = new PostmanFolderItem(rootName, 
            collection.Items.Select(i => ProcessItem(i, folderItems, rootName, args)).ToList(), collection);
        folderItems.Insert(0, testTree);
    }

    public IBddProject LoadProject(BddProjectLoaderArgs args)
    {
        var api = new PostmanApi(PostmanApiConnectionFactory.Instance.Create(args.Tracer));

        Collection collection;
        try
        {
            collection = api.GetCollection(_parameters.CollectionId).Collection;
        }
        catch (RestApiResponseException ex)
        {
            throw new SpecSyncException("Unable to load collection from Postman.", ex);
        }

        var folderItems = new List<PostmanFolderItem>();

        CreateRootItem(collection, folderItems, args);

        return new PostmanProject(folderItems, args.BaseFolder);
    }

    public string GetSourceDescription(BddProjectLoaderArgs args) 
        => $"Postman Collection '{_parameters.CollectionId}'";
}