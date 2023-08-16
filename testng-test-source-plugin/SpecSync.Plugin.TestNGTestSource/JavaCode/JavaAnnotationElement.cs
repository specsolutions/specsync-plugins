using System.Linq;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.TestNGTestSource.JavaCode;

public class JavaAnnotationElement
{
    public string Name { get; }
    public object Value { get; }
    public CodeSpan ValueSpan { get; }

    public JavaAnnotationElement(string name, object value, CodeSpan valueSpan)
    {
        Name = name;
        Value = value;
        ValueSpan = valueSpan;
    }
    public JavaAnnotationElement(object value, CodeSpan valueSpan) : this(null, value, valueSpan)
    {
    }

    public JavaAnnotationElement[] GetElementArrayValue()
    {
        if (Value is JavaAnnotationElement[] arrayValue)
            return arrayValue;
        return null;
    }

    public string[] GetStringArrayValue()
    {
        if (Value is object[] arrayValue)
            return arrayValue.Select(av => av?.ToString()).ToArray();
        return null;
    }

    public string GetStringValue()
    {
        return GetStringValue(Value);
    }

    private string GetStringValue(object value)
    {
        if (value is object[] arrayValue)
        {
            return $"{{{string.Join(",", arrayValue.Select(GetStringValue))}}}";
        }

        return value?.ToString();
    }

    public override string ToString()
    {
        return GetStringValue();
    }
}