using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using SpecSync.PluginDependency.TypeScriptSource.TypeScriptCode.TsxGrammar;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.JestTestSource.TypeScriptCode;

public class TypeScriptFunctionCallBlockParser
{
    public TypeScriptFunctionCallBlock[] Parse(string cppCode, Action<IParseTree, TsxParser, string>? onAstParsed = null)
    {
        var codeFile = new CodeFile(cppCode);
        return Parse(codeFile, onAstParsed);
    }

    public TypeScriptFunctionCallBlock[] Parse(CodeFile codeFile, Action<IParseTree, TsxParser, string>? onAstParsed = null)
    {
        var inputStream = new AntlrInputStream(codeFile.SourceCode);
        var outputWriter = new StringWriter();
        var lexer = new TsxLexer(inputStream, outputWriter, outputWriter);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new TsxParser(tokenStream, outputWriter, outputWriter)
        {
            BuildParseTree = true
        };

        var tree = parser.program();
        var parserOutput = outputWriter.ToString();
        onAstParsed?.Invoke(tree, parser, parserOutput);

        var blockParserVisitor = new TypeScriptFunctionCallBlockParserVisitor(codeFile, tokenStream);
        blockParserVisitor.VisitProgram(tree);

        return blockParserVisitor.Result.ToArray();
    }
}