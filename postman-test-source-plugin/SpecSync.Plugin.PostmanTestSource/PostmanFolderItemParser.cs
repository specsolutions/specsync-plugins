using System;
using System.Collections.Generic;
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
            testItem.Tags = ParseTagsFromMetadata(testItem.Metadata);
            testItem.TestCaseLink = ParseTestCaseLinkFromMetadata(testItem.Metadata, args);
        }

        var postmanProject = ((PostmanProject)args.BddProject);
        folderItem.Updater = new PostmanTestUpdater(postmanProject.PostmanApi, postmanProject.CollectionId);
        return folderItem;
    }

    private ILocalTestCaseTag[] ParseTagsFromMetadata(PostmanItemMetadata metadata)
    {
        var result = new List<ILocalTestCaseTag>();
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

        return result.ToArray();
    }

    private TestCaseLink ParseTestCaseLinkFromMetadata(PostmanItemMetadata metadata, LocalTestCaseContainerParseArgs args)
    {
        return GetTestCaseLinkFromMetadata(metadata, args.Configuration);
    }

    public static TestCaseLink GetTestCaseLinkFromMetadata(PostmanItemMetadata metadata, SpecSyncConfiguration configuration)
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

        return null;
    }
}