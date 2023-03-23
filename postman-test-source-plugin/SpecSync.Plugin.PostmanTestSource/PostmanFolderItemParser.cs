using System;
using System.Collections.Generic;
using System.Linq;
using SpecSync.Configuration;
using SpecSync.Parsing;
using SpecSync.Plugin.PostmanTestSource.Projects;
using SpecSync.Synchronization;
using SpecSync.Utils;

namespace SpecSync.Plugin.PostmanTestSource;

public class PostmanFolderItemParser : ILocalTestCaseContainerParser
{
    public string ServiceDescription => "Postman Collection Parser";

    public bool CanProcess(LocalTestCaseContainerParseArgs args)
        => args.SourceFile is PostmanFolderItem;

    public ILocalTestCaseContainer Parse(LocalTestCaseContainerParseArgs args)
    {
        var folderItem = args.SourceFile as PostmanFolderItem ?? throw new SpecSyncException("The parser can only be used for Postman projects");

        foreach (var testItem in folderItem.Tests)
        {
            testItem.Tags = ParseTagsFromMetadata(testItem.Metadata, testItem.ParentMetadata);
            testItem.TestCaseLink = ParseTestCaseLinkFromMetadata(testItem, args);
        }

        var postmanProject = (PostmanProject)args.BddProject;
        folderItem.Updater = new PostmanTestUpdater(postmanProject.PostmanApi, postmanProject.Parameters);
        return folderItem;
    }

    private ILocalTestCaseTag[] ParseTagsFromMetadata(PostmanItemMetadata itemMetadata, PostmanItemMetadata[] parentMetadata)
    {
        var metadataList = GetMetadataList(itemMetadata, parentMetadata);

        var result = new List<ILocalTestCaseTag>();

        foreach (var metadata in metadataList)
        {
            if (metadata.TryGetValue("tags", out MetadataListValue tagsValue))
            {
                foreach (var item in tagsValue.Items)
                {
                    result.Add(new CodeFileLocalTestCaseTag(item.StringValue, item.Span));
                }
            }
            if (metadata.TryGetValue("links", out MetadataListValue linksValue))
            {
                foreach (var item in linksValue.Items)
                {
                    result.Add(new CodeFileLocalTestCaseTag(item.StringValue, item.Span));
                }
            }
        }

        return result.GroupBy(t => t.Name).Select(g => g.First()).ToArray();
    }

    private TestCaseLink ParseTestCaseLinkFromMetadata(PostmanTestItem testItem, LocalTestCaseContainerParseArgs args)
    {
        return GetTestCaseLinkFromMetadata(testItem.Metadata, testItem.ParentMetadata, args.Configuration);
    }

    public static TestCaseLink GetTestCaseLinkFromMetadata(PostmanItemMetadata itemMetadata, IEnumerable<PostmanItemMetadata> parentMetadata, SpecSyncConfiguration configuration)
    {
        var metadataList = GetMetadataList(itemMetadata, parentMetadata);

        foreach (var metadata in metadataList)
        {
            if (configuration.Customizations.BranchTag.Enabled &&
                metadata.TryGetValue(configuration.Customizations.BranchTag.Prefix, out var branchTagValue))
            {
                return new TestCaseLink(TestCaseIdentifier.CreateExisting(branchTagValue.StringValue),
                    configuration.Synchronization.TestCaseTagPrefix);
            }

            if (metadata.TryGetValue(configuration.Synchronization.TestCaseTagPrefix, out var value))
            {
                return new TestCaseLink(TestCaseIdentifier.CreateExisting(value.StringValue),
                    configuration.Synchronization.TestCaseTagPrefix);
            }
        }

        return null;
    }

    private static IEnumerable<PostmanItemMetadata> GetMetadataList(PostmanItemMetadata itemMetadata, IEnumerable<PostmanItemMetadata> parentMetadata)
    {
        return parentMetadata.Append(itemMetadata).Reverse();
    }
}