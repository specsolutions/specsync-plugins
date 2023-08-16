using System;
using System.Diagnostics;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using JavaParserPlay.JavaCode.JavaGrammar;
using SpecSync.Utils.Code;
using System.Text.RegularExpressions;

namespace SpecSync.Plugin.TestNGTestSource.JavaCode;

public class JavaMethodBlockParser
{
    public JavaMethodBlock[] Parse(string cppCode)
    {
        var codeFile = new CodeFile(cppCode);
        return Parse(codeFile);
    }

    public JavaMethodBlock[] Parse(CodeFile codeFile)
    {
        var javaInputStream = new AntlrInputStream(codeFile.SourceCode);
        var lexer = new JavaLexer(javaInputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new JavaParser(tokenStream)
        {
            BuildParseTree = true
        };

        var tree = parser.compilationUnit();

        var blockParserVisitor = new JavaMethodBlockParserVisitor(codeFile, tokenStream.GetTokens());
        blockParserVisitor.VisitCompilationUnit(tree);

        return blockParserVisitor.Result.ToArray();
    }

    [Conditional("DEBUG")]
    // ReSharper disable once UnusedMember.Local
    private static void DumpTree(IParseTree tree, JavaParser parser)
    {
        var stringTree = tree.ToStringTree(parser);
        int nesting = 0;
        var indentedStringTree = Regex.Replace(stringTree, @"[\(\) ]", match =>
        {
            if (match.Value == " ")
                return Environment.NewLine + new string(' ', nesting * 2);

            if (match.Value == "(")
            {
                nesting++;
            }
            else
            {
                nesting--;
                return Environment.NewLine + new string(' ', nesting * 2) + ")";
            }
            return match.Value;
        });
        Console.WriteLine(indentedStringTree);
    }
}