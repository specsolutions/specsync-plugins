using SpecSync.Projects;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SpecSync.Expressions;
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
        _postmanMetadataParser = postmanMetadataParser ?? new PostmanMetadataParser(_parameters);
    }

    private bool IsTestItem(Item item, PostmanItemMetadata metadata, BddProjectLoaderArgs args, Stack<PostmanItemMetadata> parentMetadata)
    {
        // if it is already linked to a Test Case
        if (PostmanFolderItemParser.GetTestCaseLinkFromMetadata(metadata, parentMetadata, args.Configuration) != null)
            return true;

        bool MatchTestByRegex(string value, Regex regex)
        {
            if (regex != null && value != null)
            {
                var match = regex.Match(value);
                if (match.Success)
                {
                    if (match.Groups["id"].Success)
                    {
                        var id = match.Groups["id"].Value;
                        metadata.AddProperty(new MetadataProperty(args.Configuration.Synchronization.TestCaseTagPrefix, null, new MetadataStringValue(id, null)));
                    }

                    return true;
                }
            }

            return false;
        }

        if (MatchTestByRegex(item.Request?.Description ?? item.Description, _parameters.TestDocumentationRegexParsed))
            return true;
        if (MatchTestByRegex(item.Name, _parameters.TestNameRegexParsed))
            return true;

        // if it is an end-request
        if (item.Request != null)
            return true;

        return false;
    }

    private IPostmanItem ProcessItem(Item item, List<PostmanFolderItem> folderItems, string rootName, BddProjectLoaderArgs args, Stack<PostmanItemMetadata> parentMetadata)
    {
        var itemPath = rootName == null ? item.Name : $"{rootName}/{item.Name}";
        var metadata = _postmanMetadataParser.ParseMetadata(item);

        var isTestItem = IsTestItem(item, metadata, args, parentMetadata);
        if (isTestItem)
        {
            var testItem = new PostmanTestItem(item, metadata, parentMetadata.ToArray());
            return testItem;
        }

        var folderInsertIndex = folderItems.Count;
        parentMetadata.Push(metadata);

        var subPostmanItems = new List<IPostmanItem>();
        if (item.Items != null)
            foreach (var subItem in item.Items)
            {
                subPostmanItems.Add(ProcessItem(subItem, folderItems, itemPath, args, parentMetadata));
            }

        parentMetadata.Pop();
        var folderItem = new PostmanFolderItem(itemPath, subPostmanItems, item, metadata);
        if (folderItem.Tests.Any() || !folderItem.SubFolders.Any()) // contains tests or empty
            folderItems.Insert(folderInsertIndex, folderItem);
        return folderItem;
    }

    public IBddProject LoadProject(BddProjectLoaderArgs args)
    {
        var api = new PostmanApi(PostmanApiConnectionFactory.Instance.Create(args.Tracer, _parameters.PostmanApiKey));

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

        ProcessItem(collection.ToItem(), folderItems, null, args, new Stack<PostmanItemMetadata>());

        return new PostmanProject(folderItems, args.BaseFolder, api, _parameters);
    }

    public string GetSourceDescription(BddProjectLoaderArgs args) 
        => $"Postman Collection '{_parameters.CollectionId}'";
}