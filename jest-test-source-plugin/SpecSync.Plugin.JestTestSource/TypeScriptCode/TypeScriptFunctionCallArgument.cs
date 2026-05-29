using SpecSync.Utils.Code;

namespace SpecSync.Plugin.JestTestSource.TypeScriptCode;

public class TypeScriptFunctionCallArgument(string text, CodeSpan span)
{
    public interface IArrayElement;
    public class LiteralArrayElement(string value) : IArrayElement
    {
        public string Value { get; } = value;
        public override string ToString() => Value;
    }

    public class ArrayArgument : List<IArrayElement>, IArrayElement
    {
        public override string ToString() => string.Join(",", this);
    }

    public CodeSpan Span { get; } = span;
    public string Text => text;
    public string? StringLiteral { get; set; }
    public List<TypeScriptFunctionCallBlock>? NestedCallBlocks { get; set; }
    public List<string>? LambdaArgNames { get; set; }
    public bool IsLambda { get; set; }
    public ArrayArgument? Array { get; set; }
    public bool IsArray => Array != null;

    public override string ToString()
    {
        return IsLambda ? "(...) => {...}" : 
            IsArray ? "[...]" : 
            Text;
    }

    public void AddNestedCall(TypeScriptFunctionCallBlock callBlock)
    {
        NestedCallBlocks ??= new();
        NestedCallBlocks.Add(callBlock);
    }
}