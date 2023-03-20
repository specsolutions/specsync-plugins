using SpecSync.Parsing;
using SpecSync.Plugin.PostmanTestSource.Postman.Models;

namespace SpecSync.Plugin.PostmanTestSource.Projects;

public class PostmanTestItem : IPostmanItem, ILocalTestCase
{
    private readonly Item _modelItem;

    public PostmanTestItem(Item modelItem)
    {
        _modelItem = modelItem;
    }

    #region ILocalTestCase implementation

    public string Name => _modelItem.Name;
    public string Description => null;
    public string TestedRule => null;
    public ILocalTestCaseTag[] Tags { get; }
    public TestCaseLink TestCaseLink { get; }
    public bool IsDataDrivenTest => false;
    public LocalTestCaseDataRow[] DataRows => null;
    public int TestCount => 1;

    #endregion
}