using System.Diagnostics;
using SpecSync.Parsing;
using SpecSync.Plugin.PostmanTestSource.Postman.Models;

namespace SpecSync.Plugin.PostmanTestSource.Projects;

[DebuggerDisplay("{Name}")]
public class PostmanTestItem(
    Item modelItem,
    PostmanItemMetadata metadata,
    PostmanItemMetadata[]? parentMetadata = null)
    : IPostmanItem, ILocalTestCase
{
    public Item ModelItem { get; } = modelItem;
    public PostmanItemMetadata[] ParentMetadata { get; } = parentMetadata ?? [];
    public PostmanItemMetadata Metadata { get; } = metadata;

    public IEnumerable<Item> GetRequestItems()
    {
        IEnumerable<Item> GetRequestItemsInternal(Item item)
        {
            if (item.Items == null || item.Items.Length == 0)
            {
                if (item.Request != null)
                    yield return item;
            }
            else
                foreach (var subItem in item.Items.SelectMany(GetRequestItemsInternal))
                {
                    yield return subItem;
                }
        }

        return GetRequestItemsInternal(ModelItem);
    }

    #region ILocalTestCase implementation

    public ILocalArtifactTag[] Tags { get; set; } = [];
    public IdLink? IdLink { get; set; } = null;
    public string Name => ModelItem.Name;
    public string Description => Metadata.CleanedDocumentation;
    public AcceptanceCriterion? TestedRule => null;
    public LocalTestCaseDataRow[]? DataRows => null;
    public string[]? ParameterNames => null;
    public int TestCount => 1;

    #endregion
}