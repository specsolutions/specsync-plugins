using SpecSync.Utils.Code;

namespace SpecSync.Plugin.PostmanTestSource.Projects;

/// <summary>
/// The metadata extracted from a Postman item documentation.
/// </summary>
/// <param name="documentationContent">The full item documentation as an in-memory code-file.</param>
/// <param name="metadataHeadingName">The name of the heading used to separate the SpecSync related metadata.</param>
/// <param name="metaHeadingSpan">The span of the SpecSync related metadata part, or <c>null</c> if not available.</param>
/// <param name="cleanedDocumentation">The documentation without the SpecSync related part.</param>
public class PostmanItemMetadata(
    EditableCodeFile documentationContent,
    string metadataHeadingName,
    CodeSpan? metaHeadingSpan,
    string cleanedDocumentation)
{
    private readonly Dictionary<string, MetadataProperty> _metadataProperties = new();

    /// <summary>
    /// The full item documentation as an in-memory code-file.
    /// </summary>
    public EditableCodeFile DocumentationContent { get; set; } = documentationContent;
    /// <summary>
    /// The span of the SpecSync related metadata part, or <c>null</c> if not available.
    /// </summary>
    public CodeSpan? MetaHeadingSpan { get; set; } = metaHeadingSpan;
    /// <summary>
    /// The name of the heading used to separate the SpecSync related metadata.
    /// </summary>
    public string MetadataHeadingName { get; set; } = metadataHeadingName;
    /// <summary>
    /// The documentation without the SpecSync related part.
    /// </summary>
    public string CleanedDocumentation { get; set; } = cleanedDocumentation;

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

    public bool TryGetValue(string key, out IMetadataValue? value)
    {
        if (_metadataProperties.TryGetValue(key, out var property))
        {
            value = property.Value;
            return true;
        }

        value = null;
        return false;
    }

    public bool TryGetValue<TValue>(string key, out TValue? value) where TValue: class, IMetadataValue
    {
        if (_metadataProperties.TryGetValue(key, out var property) && property.Value is TValue valueValue)
        {
            value = valueValue;
            return true;
        }

        value = null;
        return false;
    }
}