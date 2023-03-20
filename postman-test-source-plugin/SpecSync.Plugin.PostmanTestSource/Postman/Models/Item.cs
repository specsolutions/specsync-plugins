using Newtonsoft.Json;

namespace SpecSync.Plugin.PostmanTestSource.Postman.Models;

public class Item
{
    public string Name { get; set; }

    [JsonProperty("item")] public Item[] Items { get; set; }

    public Request Request { get; set; }
}