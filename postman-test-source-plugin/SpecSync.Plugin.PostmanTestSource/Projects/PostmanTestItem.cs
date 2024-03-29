﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SpecSync.Parsing;
using SpecSync.Plugin.PostmanTestSource.Postman.Models;

namespace SpecSync.Plugin.PostmanTestSource.Projects;

[DebuggerDisplay("{Name}")]
public class PostmanTestItem : IPostmanItem, ILocalTestCase
{
    public Item ModelItem { get; }
    public PostmanItemMetadata[] ParentMetadata { get; }
    public PostmanItemMetadata Metadata { get; }

    public PostmanTestItem(Item modelItem, PostmanItemMetadata metadata, PostmanItemMetadata[] parentMetadata = null)
    {
        ModelItem = modelItem;
        ParentMetadata = parentMetadata ?? Array.Empty<PostmanItemMetadata>();
        Metadata = metadata ?? new PostmanItemMetadata();
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

        return GetRequestItemsInternal(ModelItem);
    }

    #region ILocalTestCase implementation

    public ILocalTestCaseTag[] Tags { get; set; }
    public TestCaseLink TestCaseLink { get; set; }
    public string Name => ModelItem.Name;
    public string Description => Metadata.CleanedDocumentation ?? ModelItem.Description ?? ModelItem.Request?.Description;
    public string TestedRule => null;
    public bool IsDataDrivenTest => false;
    public LocalTestCaseDataRow[] DataRows => null;
    public int TestCount => 1;

    #endregion
}