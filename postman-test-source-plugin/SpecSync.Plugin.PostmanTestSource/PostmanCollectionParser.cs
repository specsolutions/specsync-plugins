using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SpecSync.Parsing;
using SpecSync.Plugin.PostmanTestSource.Projects;
using SpecSync.Synchronization;
using SpecSync.Utils;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.PostmanTestSource;

public class PostmanCollectionParser : ILocalTestCaseContainerParser
{
    public string ServiceDescription => "Postman Collection Parser";

    public bool CanProcess(LocalTestCaseContainerParseArgs args)
        => args.SourceFile is PostmanFolderItem;

    public ILocalTestCaseContainer Parse(LocalTestCaseContainerParseArgs args)
    {
        var folderItem = args.SourceFile as PostmanFolderItem ?? throw new SpecSyncException("The parser can only be used for Postman projects");

        foreach (var testItem in folderItem.Tests)
        {
            var documentationContent = new EditableCodeFile(new InMemoryWritableTextFile(testItem.Description ?? ""));
            testItem.DocumentationContent = documentationContent;
            var metadata = ParseMetadataFromDocumentation(documentationContent);
            foreach (var metadataItem in metadata)
            {
                testItem.Metadata[metadataItem.Key] = metadataItem.Value;
            }

            testItem.Tags = ParseTagsFromMetadata(testItem.Metadata);
            testItem.TestCaseLink = ParseTestCaseLinkFromMetadata(testItem.Metadata, args);
        }

        return folderItem;
    }

    private ILocalTestCaseTag[] ParseTagsFromMetadata(Dictionary<string, IMetadataValue> metadata)
    {
        var result = new List<ILocalTestCaseTag>();
        if (metadata.TryGetValue("tags", out var tagsValue) && tagsValue is MetadataListValue tagsListValue)
        {
            foreach (var item in tagsListValue.Items)
            {
                result.Add(new CodeFileLocalTestCaseTag(item.StringValue, item.Span));
            }
        }
        if (metadata.TryGetValue("links", out var linksValue) && linksValue is MetadataListValue linksListValue)
        {
            foreach (var item in linksListValue.Items)
            {
                result.Add(new CodeFileLocalTestCaseTag(item.StringValue, item.Span));
            }
        }

        return result.ToArray();
    }

    private TestCaseLink ParseTestCaseLinkFromMetadata(Dictionary<string, IMetadataValue> metadata, LocalTestCaseContainerParseArgs args)
    {
        if (args.Configuration.Customizations.BranchTag.Enabled &&
            metadata.TryGetValue(args.Configuration.Customizations.BranchTag.Prefix, out var branchTagValue))
        {
            return new TestCaseLink(TestCaseIdentifier.CreateExisting(branchTagValue.StringValue), args.Configuration.Synchronization.TestCaseTagPrefix);
        }
        if (metadata.TryGetValue(args.Configuration.Synchronization.TestCaseTagPrefix, out var value))
        {
            return new TestCaseLink(TestCaseIdentifier.CreateExisting(value.StringValue), args.Configuration.Synchronization.TestCaseTagPrefix);
        }
        return null;
    }

    private static readonly Regex MarkdownHeadingLine = new(@"^#+\s+(?<name>.*?)\s*$");
    private static readonly Regex MarkdownListLine = new(@"^(?<indent>\s*)-\s+((?<key>[^:]+?)\s*:\s*)?(?<value>\S.*?)?\s*$");
    private static readonly Regex MarkdownLinkRe = new(@"^\[(?<linkText>.+)\]\((?<url>.+)\)$");

    private List<MetadataProperty> ParseMetadataFromDocumentation(EditableCodeFile documentationContent)
    {
        CodeSpan GetSpan(Group match, int lineIndex)
        {
            return new CodeSpan(documentationContent, new CodePosition(lineIndex, match.Index), match.Length);
        }

        var rootList = new List<IMetadataValue>();
        var listsOfLevels = new Dictionary<int, List<IMetadataValue>>
        {
            { 0, rootList }
        };
        bool inMetaSection = false;
        for (int lineIndex = 0; lineIndex < documentationContent.LineCount; lineIndex++)
        {
            var line = documentationContent.GetLine(lineIndex);
            var heading = MarkdownHeadingLine.Match(line);
            if (heading.Success)
            {
                inMetaSection = IsMetaHeading(heading.Groups["name"].Value);
                continue;
            }

            if (inMetaSection)
            {
                var listLine = MarkdownListLine.Match(line);
                if (listLine.Success)
                {
                    var level = listLine.Groups["indent"].Length == 0 ? 0 : 1;
                    var parentList = listsOfLevels[level];
                    IMetadataValue value = null;
                    CodeSpan valueSpan = null;
                    if (listLine.Groups["value"].Success)
                    {
                        var stringValue = listLine.Groups["value"].Value;
                        valueSpan = GetSpan(listLine.Groups["value"], lineIndex);
                        var link = MarkdownLinkRe.Match(stringValue);
                        if (link.Success)
                            value = new MetadataLinkValue(link.Groups["linkText"].Value, link.Groups["url"].Value, valueSpan);
                        else
                            value = new MetadataStringValue(stringValue, valueSpan);
                    }

                    if (listLine.Groups["key"].Success)
                    {
                        var keySpan = GetSpan(listLine.Groups["key"], lineIndex);
                        if (value == null)
                        {
                            var list = new MetadataListValue();
                            listsOfLevels[level + 1] = list.Items;
                            value = list;
                        }

                        CodeSpan span = null;
                        if (valueSpan != null)
                            span = new CodeSpan(documentationContent, keySpan.Start, valueSpan.End);

                        parentList.Add(new MetadataProperty(listLine.Groups["key"].Value, keySpan, value, span));
                    }
                    else if (value != null)
                    {
                        parentList.Add(value);
                    }
                }
            }
        }

        return rootList.OfType<MetadataProperty>().ToList();
    }

    private bool IsMetaHeading(string value)
    {
        return value.Equals("Metadata", StringComparison.InvariantCultureIgnoreCase);
    }
}