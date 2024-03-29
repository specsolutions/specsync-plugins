﻿using Newtonsoft.Json;

namespace SpecSync.Plugin.PostmanTestSource.Postman.Models;

public class Item
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    [JsonProperty("item")] public Item[] Items { get; set; }

    public Request Request { get; set; }
    [JsonProperty("event")] public Event[] Events { get; set; }
}