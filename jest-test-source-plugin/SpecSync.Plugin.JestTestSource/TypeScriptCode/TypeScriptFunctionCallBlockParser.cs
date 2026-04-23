using System.Diagnostics;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using SpecSync.PluginDependency.TypeScriptSource.TypeScriptCode.TypeScriptGrammar;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.JestTestSource.TypeScriptCode;

public class TypeScriptFunctionCallBlockParser
{
    public TypeScriptFunctionCallBlock[] Parse(string cppCode, Action<IParseTree, TypeScriptParser>? onAstParsed = null)
    {
        var codeFile = new CodeFile(cppCode);
        return Parse(codeFile, onAstParsed);
    }

    public TypeScriptFunctionCallBlock[] Parse(CodeFile codeFile, Action<IParseTree, TypeScriptParser>? onAstParsed = null)
    {
        var inputStream = new AntlrInputStream(codeFile.SourceCode);
        var lexer = new TypeScriptLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new TypeScriptParser(tokenStream)
        {
            BuildParseTree = true
        };

        var tree = parser.program();
        onAstParsed?.Invoke(tree, parser);

        var blockParserVisitor = new TypeScriptFunctionCallBlockParserVisitor(codeFile, tokenStream);
        blockParserVisitor.VisitProgram(tree);

        return blockParserVisitor.Result.ToArray();
    }
}