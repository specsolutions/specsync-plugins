using Newtonsoft.Json.Linq;
using SpecSync.Parsing;
using SpecSync.Plugin.PostmanTestSource.Postman;
using SpecSync.Plugin.PostmanTestSource.Projects;

namespace SpecSync.Plugin.PostmanTestSource;

public class PostmanTestUpdater(
    PostmanApi api,
    PostmanTestSourcePlugin.Parameters parameters)
    : SourceDocumentUpdaterBase
{
    private readonly Dictionary<string, Func<JObject, bool>> _itemChanges = new();
    private bool _isDirty;

    public override bool IsDirty => _isDirty;

    public override bool Flush()
    {
        if (_itemChanges.Any())
        {
            var result = api.SafeUpdateCollection(parameters.CollectionId, _itemChanges);
            _itemChanges.Clear();
            _isDirty = false;
            return result != null;
        }

        return false;
    }

    public override void SetArtifactLink(ILocalArtifact localArtifact, IdLink idLink)
    {
        var testItem = (PostmanTestItem)localArtifact;
        var documentation = testItem.Metadata.DocumentationContent;

        var insertPosition = testItem.Metadata.MetaHeadingSpan?.End ?? documentation.GetLineEndPosition(documentation.LineCount - 1);
        var metadataPrefix = testItem.Metadata.MetaHeadingSpan == null ? $"\n## {parameters.MetadataHeading}\n\n" : "";
        var template = parameters.ResolvedTestCaseLinkTemplate;
        var link = template.Replace("{id}", idLink.Id.ToString());
        var idText = string.IsNullOrWhiteSpace(link) ? idLink.Id.ToString() : $"[{idLink.Id}]({link})";
        documentation.Updater.InsertLineAfter(insertPosition, $"{metadataPrefix}- {idLink.LinkPrefix}: {idText}");
        documentation.Save();

        if (testItem.ModelItem.Request != null)
        {
            _itemChanges[testItem.ModelItem.Id] = itemObj =>
            {
                itemObj["request"]!["description"] = documentation.UpdatedSourceCode;
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