using SpecSync.Utils.Code;

namespace SpecSync.Plugin.TestNGTestSource.JavaCode;

public class JavaAnnotationElement(string? name, object value, CodeSpan valueSpan)
{
    public string? Name { get; } = name;
    public object Value { get; } = value;
    public CodeSpan ValueSpan { get; } = valueSpan;

    public JavaAnnotationElement(object value, CodeSpan valueSpan) : this(null, value, valueSpan)
    {
    }

    public JavaAnnotationElement[]? GetElementArrayValue()
    {
        if (Value is JavaAnnotationElement[] arrayValue)
            return arrayValue;
        return null;
    }

    public string?[]? GetStringArrayValue()
    {
        if (Value is object[] arrayValue)
            return arrayValue.Select(av => av?.ToString()).ToArray();
        return null;
    }

    public string? GetStringValue()
    {
        return GetStringValue(Value);
    }

    private string? GetStringValue(object? value)
    {
        if (value is object[] arrayValue)
        {
            return $"{{{string.Join(",", arrayValue.Select(GetStringValue))}}}";
        }

        return value?.ToString();
    }

    public override string ToString()
    {
        return GetStringValue() ?? "";
    }
}