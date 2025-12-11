using Newtonsoft.Json.Linq;
using SpecSync.Integration.RestApiServices;
using SpecSync.Plugin.PostmanTestSource.Postman.Models;

namespace SpecSync.Plugin.PostmanTestSource.Postman;

public class PostmanApi(IPostmanApiConnection connection)
{
    private readonly Dictionary<string, JObject> _collectionGetCache = new();

    public GetCollectionResponse GetCollection(string collectionId)
    {
        return connection.ExecuteGet<GetCollectionResponse>($"collections/{collectionId}");
    }

    public UpdateCollectionResponse UpdateCollection(string collectionId, JObject updateCollectionPayload)
    {
        return connection.ExecutePut<UpdateCollectionResponse>($"collections/{collectionId}", updateCollectionPayload).ResponseData!;
    }

    public UpdateCollectionResponse? SafeUpdateCollection(string collectionId, Dictionary<string, Func<JObject, bool>> changes)
    {
        if (!_collectionGetCache.TryGetValue(collectionId, out var collectionResponseJObject))
        {
            connection.Tracer.LogVerbose("Loading collection for update...");
            collectionResponseJObject = connection.ExecuteGet<JObject>($"collections/{collectionId}");
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