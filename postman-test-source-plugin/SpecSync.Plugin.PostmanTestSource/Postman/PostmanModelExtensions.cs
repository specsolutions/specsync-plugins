using SpecSync.Plugin.PostmanTestSource.Postman.Models;

namespace SpecSync.Plugin.PostmanTestSource.Postman;

public static class PostmanModelExtensions
{
    public static Item ToItem(this Collection collection)
    {
        return new Item
        {
            Name = collection.Info?.Name ?? "Collection",
            Items = collection.Items,
            Description = collection.Info?.Description
        };
    }
}