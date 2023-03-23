using SpecSync.Utils.Code;
using System.Collections.Generic;

namespace SpecSync.Plugin.PostmanTestSource.Projects;

public class PostmanItemMetadata
{
    private readonly Dictionary<string, MetadataProperty> _metadataProperties = new();

    public EditableCodeFile DocumentationContent { get; set; }
    public CodeSpan MetaHeadingSpan { get; set; }
    public string MetadataHeadingName { get; set; }
    public string CleanedDocumentation { get; set; }

    public IMetadataValue this[string key] => _metadataProperties[key].Value;

    public void AddProperty(MetadataProperty property)
    {
        _metadataProperties[property.Key] = property;
    }

    public MetadataProperty GetMetadataProperty(string key)
    {
        return _metadataProperties[key];
    }

    public bool ContainsKey(string key) => _metadataProperties.ContainsKey(key);

    public bool TryGetValue(string key, out IMetadataValue value)
    {
        if (_metadataProperties.TryGetValue(key, out var property))
        {
            value = property.Value;
            return true;
        }

        value = null;
        return false;
    }

    public bool TryGetValue<TValue>(string key, out TValue value) where TValue: class, IMetadataValue
    {
        if (_metadataProperties.TryGetValue(key, out var property) && property.Value is TValue)
        {
            value = (TValue)property.Value;
            return true;
        }

        value = null;
        return false;
    }
}