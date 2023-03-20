using System.Collections.Generic;
using SpecSync.Parsing;
using SpecSync.Plugin.PostmanTestSource.Postman.Models;
using SpecSync.Utils;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.PostmanTestSource.Projects;

public class PostmanTestItem : IPostmanItem, ILocalTestCase
{
    private readonly Item _modelItem;
    public EditableCodeFile DocumentationContent { get; set; }
    public Dictionary<string, IMetadataValue> Metadata { get; } = new();

    public PostmanTestItem(Item modelItem)
    {
        _modelItem = modelItem;
    }

    #region ILocalTestCase implementation

    public ILocalTestCaseTag[] Tags { get; set; }
    public TestCaseLink TestCaseLink { get; set; }
    public string Name => _modelItem.Name;
    public string Description => _modelItem.Description ?? _modelItem.Request?.Description;
    public string TestedRule => null;
    public bool IsDataDrivenTest => false;
    public LocalTestCaseDataRow[] DataRows => null;
    public int TestCount => 1;

    #endregion
}