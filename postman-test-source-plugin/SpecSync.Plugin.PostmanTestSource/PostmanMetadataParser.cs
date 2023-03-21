using SpecSync.Plugin.PostmanTestSource.Projects;
using SpecSync.Utils.Code;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using SpecSync.Plugin.PostmanTestSource.Postman.Models;
using SpecSync.Utils;

namespace SpecSync.Plugin.PostmanTestSource;

public class PostmanMetadataParser
{
    private static readonly Regex MarkdownHeadingLine = new(@"^#+\s+(?<name>.*?)\s*$");
    private static readonly Regex MarkdownListLine = new(@"^(?<indent>\s*)-\s+((?<key>[^:]+?)\s*:\s*)?(?<value>\S.*?)?\s*$");
    private static readonly Regex MarkdownLinkRe = new(@"^\[(?<linkText>.+)\]\((?<url>.+)\)$");

    public PostmanItemMetadata ParseMetadata(Item item)
    {
        var metadata = new PostmanItemMetadata();

        var documentation = item.Request?.Description ?? item.Description ?? "";
        var documentationContent = new EditableCodeFile(new InMemoryWritableTextFile(documentation));
        metadata.DocumentationContent = documentationContent;
        var docProperties = ParseMetadataFromDocumentation(documentationContent);
        foreach (var property in docProperties)
        {
            metadata.AddProperty(property);
        }

        return metadata;
    }

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