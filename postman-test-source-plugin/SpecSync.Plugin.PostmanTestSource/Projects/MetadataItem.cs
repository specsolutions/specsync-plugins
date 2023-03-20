using System.Collections.Generic;
using System.Linq;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.PostmanTestSource.Projects;

public interface IMetadataValue
{
    string StringValue { get; }
    CodeSpan Span { get; }
}

public class MetadataStringValue : IMetadataValue
{
    public MetadataStringValue(string value, CodeSpan span)
    {
        Value = value;
        Span = span;
    }

    public string Value { get; }
    public CodeSpan Span { get; }
    string IMetadataValue.StringValue => Value;
}

public class MetadataLinkValue : IMetadataValue
{
    public MetadataLinkValue(string linkText, string url, CodeSpan span)
    {
        LinkText = linkText;
        Url = url;
        Span = span;
    }

    public CodeSpan Span { get; }
    public string LinkText { get; }
    public string Url { get; }
    public string StringValue => LinkText;
}

public class MetadataProperty : IMetadataValue
{
    public MetadataProperty(string key, CodeSpan keySpan, IMetadataValue value, CodeSpan span = null)
    {
        Key = key;
        KeySpan = keySpan;
        Value = value;
        Span = span;
    }

    public string Key { get; }
    public CodeSpan KeySpan { get; }
    public IMetadataValue Value { get; }
    public string StringValue => $"{Key}:{Value.StringValue}";
    public CodeSpan Span { get; }
}

public class MetadataListValue : IMetadataValue
{
    public List<IMetadataValue> Items { get; } = new();
    //public CodeSpan Span { get; }
    public string StringValue => string.Join(";", Items.Select(i => i.StringValue));
    public CodeSpan Span => null;
}