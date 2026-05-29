using SpecSync.Plugin.JestTestSource.TypeScriptCode.TsxGrammar;

namespace SpecSync.Plugin.JestTestSource.TypeScriptCode;

public class TypeScriptFunctionCallBlockParserResult(TypeScriptFunctionCallBlock[] result, bool success, string outputMessages, TsxToken[] tokens)
{
    public TypeScriptFunctionCallBlock[] Result { get; } = result;
    public bool Success { get; } = success;
    public string OutputMessages { get; } = outputMessages;
    public TsxToken[] Tokens { get; } = tokens;
}