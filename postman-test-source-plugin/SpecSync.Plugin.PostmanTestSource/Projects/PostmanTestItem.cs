﻿using System.Collections.Generic;
using System.Linq;
using SpecSync.Parsing;
using SpecSync.Plugin.PostmanTestSource.Postman.Models;
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

        return GetRequestItemsInternal(_modelItem);
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