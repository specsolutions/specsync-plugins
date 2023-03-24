using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using SpecSync.Integration.RestApiServices;
using SpecSync.Plugin.PostmanTestSource.Postman.Models;

namespace SpecSync.Plugin.PostmanTestSource.Postman;

public class PostmanApi
{
    private readonly IPostmanApiConnection _connection;
    private readonly Dictionary<string, JObject> _collectionGetCache = new();

    public PostmanApi(IPostmanApiConnection connection)
    {
        _connection = connection;
    }

    public GetCollectionResponse GetCollection(string collectionId)
    {
        return _connection.ExecuteGet<GetCollectionResponse>($"collections/{collectionId}");
    }

    public UpdateCollectionResponse UpdateCollection(string collectionId, JObject updateCollectionPayload)
    {
        return _connection.ExecutePut<UpdateCollectionResponse>($"collections/{collectionId}", updateCollectionPayload).ResponseData;
    }

    public UpdateCollectionResponse SafeUpdateCollection(string collectionId, Dictionary<string, Func<JObject, bool>> changes)
    {
        if (!_collectionGetCache.TryGetValue(collectionId, out var collectionResponseJObject))
        {
            _connection.Tracer?.LogVerbose("Loading collection for update...");
            collectionResponseJObject = _connection.ExecuteGet<JObject>($"collections/{collectionId}");
            _collectionGetCache[collectionId] = collectionResponseJObject;
        }

        bool hasChanged = false;
        if (collectionResponseJObject["collection"] is JObject rootItem)
        {
            var itemsQueue = new Queue<JObject>();
            itemsQueue.Enqueue(rootItem);

            while (itemsQueue.Any())
            {
                var item = itemsQueue.Dequeue();
                var itemId = item == rootItem ? collectionId : (item["id"] as JValue)?.Value as string;
                if (itemId != null && changes.TryGetValue(itemId, out var change))
                {
                    if (change(item))
                        hasChanged = true;
                }

                if (item["item"] is JArray subItems)
                {
                    foreach (var subItem in subItems.OfType<JObject>())
                    {
                        itemsQueue.Enqueue(subItem);
                    }
                }
            }
        }

        if (hasChanged)
            return UpdateCollection(collectionId, collectionResponseJObject);

        return null;
    }
}