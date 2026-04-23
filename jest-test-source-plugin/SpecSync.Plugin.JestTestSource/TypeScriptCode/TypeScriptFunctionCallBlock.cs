using System.Text;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.JestTestSource.TypeScriptCode;

public class TypeScriptFunctionCallBlock(
    string functionName,
    TypeScriptFunctionCallArgument[]? callArguments,
    bool isSimpleCall,
    List<List<TypeScriptFunctionCallArgument>> targetArguments,
    CodeSpan sourceSpan,
    CodeSpan? callCommentSpan)
{
    public string FunctionName { get; } = functionName;
    public bool IsSimpleCall { get; } = isSimpleCall;
    public List<List<TypeScriptFunctionCallArgument>> TargetArguments { get; } = targetArguments;

    public TypeScriptFunctionCallArgument[] CallArguments = callArguments ?? [];

    public CodeSpan SourceSpan { get; } = sourceSpan;
    public CodeSpan? CallCommentSpan { get; } = callCommentSpan;

    public override string ToString()
    {
        var result = new StringBuilder();
        result.Append(IsSimpleCall ? $"`#{FunctionName}`" : $"`{FunctionName}<{string.Join(";", TargetArguments.Select(ta => string.Join(",", ta.Select(ca => ca.ToString()))))}>`");
        result.Append("(");
        result.Append(string.Join(",", CallArguments.Select(ca => ca.ToString())));
        result.Append(")");
        return result.ToString();
    }

}