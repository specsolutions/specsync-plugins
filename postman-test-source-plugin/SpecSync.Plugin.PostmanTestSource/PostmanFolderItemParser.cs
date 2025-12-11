using SpecSync.Configuration;
using SpecSync.Parsing;
using SpecSync.Plugin.PostmanTestSource.Projects;
using SpecSync.Synchronization;
using SpecSync.Utils;

namespace SpecSync.Plugin.PostmanTestSource;

public class PostmanFolderItemParser : ISourceDocumentParser
{
    public string ServiceDescription => "Postman Collection Parser";

    public bool CanProcess(SourceDocumentParserArgs args)
        => args.SourceReference is PostmanFolderItem;

    public ISourceDocument Parse(SourceDocumentParserArgs args)
    {
        var folderItem = args.SourceReference as PostmanFolderItem ?? throw new SpecSyncException("The parser can only be used for Postman projects");

        foreach (var testItem in folderItem.Tests)
        {
            testItem.Tags = ParseTagsFromMetadata(testItem.Metadata, testItem.ParentMetadata);
            testItem.IdLink = ParseTestCaseLinkFromMetadata(testItem, args);
        }

        var postmanProject = (PostmanProject)args.Project;
        folderItem.Updater = new PostmanTestUpdater(postmanProject.PostmanApi, postmanProject.Parameters);
        return folderItem;
    }

    private ILocalArtifactTag[] ParseTagsFromMetadata(PostmanItemMetadata itemMetadata, PostmanItemMetadata[] parentMetadata)
    {
        var metadataList = GetMetadataList(itemMetadata, parentMetadata);

        var result = new List<ILocalArtifactTag>();

        foreach (var metadata in metadataList)
        {
            ILocalArtifactTag CreateTag(IMetadataValue value)
            {
                return value.Span != null ? 
                    new CodeFileLocalArtifactTag(value.StringValue, value.Span) : 
                    new LocalArtifactTag(value.StringValue);
            }

            if (metadata.TryGetValue("tags", out MetadataListValue? tagsValue))
            {
                foreach (var item in tagsValue!.Items)
                {
                    result.Add(CreateTag(item));
                }
            }
            if (metadata.TryGetValue("links", out MetadataListValue? linksValue))
            {
                foreach (var item in linksValue!.Items)
                {
                    result.Add(CreateTag(item));
                }
            }
        }

        return result.GroupBy(t => t.Name).Select(g => g.First()).ToArray();
    }

    private IdLink? ParseTestCaseLinkFromMetadata(PostmanTestItem testItem, SourceDocumentParserArgs args)
    {
        return GetTestCaseLinkFromMetadata(testItem.Metadata, testItem.ParentMetadata, args.Configuration);
    }

    public static IdLink? GetTestCaseLinkFromMetadata(PostmanItemMetadata itemMetadata, IEnumerable<PostmanItemMetadata> parentMetadata, SpecSyncConfiguration configuration)
    {
        var metadataList = GetMetadataList(itemMetadata, parentMetadata);

        foreach (var metadata in metadataList)
        {
            if (configuration.Customizations.BranchTag.Enabled &&
                metadata.TryGetValue(configuration.Customizations.BranchTag.Prefix, out var branchTagValue))
            {
                return new IdLink(TestCaseIdentifier.CreateExisting(branchTagValue!.StringValue),
                    configuration.Synchronization.TestCaseTagPrefix);
            }

            if (metadata.TryGetValue(configuration.Synchronization.TestCaseTagPrefix, out var value))
            {
                return new IdLink(TestCaseIdentifier.CreateExisting(value!.StringValue),
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