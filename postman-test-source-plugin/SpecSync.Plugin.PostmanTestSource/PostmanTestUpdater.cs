using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using SpecSync.Parsing;
using SpecSync.Plugin.PostmanTestSource.Postman;
using SpecSync.Plugin.PostmanTestSource.Projects;

namespace SpecSync.Plugin.PostmanTestSource;

public class PostmanTestUpdater : LocalTestCaseContainerUpdaterBase
{
    private readonly string _collectionId;
    private readonly PostmanApi _api;
    private readonly Dictionary<string, Func<JObject, bool>> _itemChanges = new();
    private bool _isDirty;

    public override bool IsDirty => _isDirty;

    public PostmanTestUpdater(PostmanApi api, string collectionId)
    {
        _api = api;
        _collectionId = collectionId;
    }

    public override bool Flush()
    {
        if (_itemChanges.Any())
        {
            var result = _api.SafeUpdateCollection(_collectionId, _itemChanges);
            _itemChanges.Clear();
            _isDirty = false;
            return result != null;
        }

        return false;
    }

    public override void SetTestCaseLink(ILocalTestCase localTestCase, TestCaseLink testCaseLink)
    {
        var testItem = (PostmanTestItem)localTestCase;
        var documentation = testItem.Metadata.DocumentationContent;

        var insertPosition = testItem.Metadata.MetaHeadingSpan?.End ?? documentation.GetLineEndPosition(documentation.LineCount - 1);
        var metadataPrefix = testItem.Metadata.MetaHeadingSpan == null ? "\n## Metadata\n\n" : "";
        documentation.Updater.InsertLineAfter(insertPosition, $"{metadataPrefix}- {testCaseLink.LinkPrefix}: {testCaseLink.TestCaseId}");
        documentation.Save();

        if (testItem.ModelItem.Request != null)
        {
            _itemChanges[testItem.ModelItem.Id] = itemObj =>
            {
                itemObj["request"]["description"] = documentation.UpdatedSourceCode;
                return true;
            };
        }
        else
        {
            _itemChanges[testItem.ModelItem.Id] = itemObj =>
            {
                itemObj["description"] = documentation.UpdatedSourceCode;
                return true;
            };
        }

        _isDirty = true;
    }
}