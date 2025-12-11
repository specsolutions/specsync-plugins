using System.Diagnostics;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.PostmanTestSource.Projects;

public interface IMetadataValue
{
    string StringValue { get; }
    CodeSpan? Span { get; }
}

[DebuggerDisplay("{Value}")]
public class MetadataStringValue(string value, CodeSpan? span) : IMetadataValue
{
    public string Value { get; } = value;
    public CodeSpan? Span { get; } = span;
    string IMetadataValue.StringValue => Value;
}

[DebuggerDisplay("[{LinkText}]({Url})")]
public class MetadataLinkValue(string linkText, string url, CodeSpan span) : IMetadataValue
{
    public CodeSpan Span { get; } = span;
    public string LinkText { get; } = linkText;
    public string Url { get; } = url;
    public string StringValue => LinkText;
}

[DebuggerDisplay("{StringValue}")]
public class MetadataProperty(string key, CodeSpan? keySpan, IMetadataValue value, CodeSpan? span = null)
    : IMetadataValue
{
    public string Key { get; } = key;
    public CodeSpan? KeySpan { get; } = keySpan;
    public IMetadataValue Value { get; } = value;
    public string StringValue => $"{Key}:{Value.StringValue}";
    public CodeSpan? Span { get; } = span;
}

[DebuggerDisplay("[{StringValue}]")]
public class MetadataListValue : IMetadataValue
{
    public List<IMetadataValue> Items { get; } = new();
    //public CodeSpan Span { get; }
    public string StringValue => string.Join(";", Items.Select(i => i.StringValue));
    public CodeSpan? Span => null;
}