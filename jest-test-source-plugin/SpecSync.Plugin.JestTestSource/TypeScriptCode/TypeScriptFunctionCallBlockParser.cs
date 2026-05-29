using SpecSync.Plugin.JestTestSource.TypeScriptCode.TsxGrammar;
using SpecSync.Utils;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.JestTestSource.TypeScriptCode;

public class TypeScriptFunctionCallBlockParser
{
    public TypeScriptFunctionCallBlockParserResult Parse(string cppCode)
    {
        var codeFile = new CodeFile(cppCode);
        return Parse(codeFile);
    }

    public TypeScriptFunctionCallBlockParserResult Parse(CodeFile codeFile)
    {
        var tokenizer = new TsxTokenizer(codeFile.SourceCode);
        var tokens = tokenizer.Tokenize().ToArray();
        var errorMessages = new List<string>();

        bool NextTokenIs(int index, TsxTokenKind expectedKind, int next = 1, Func<TsxToken, bool>? additionalCheck = null)
        {
            var checkIndex = index + next;
            return checkIndex >= 0 && checkIndex < tokens.Length && tokens[checkIndex].Kind == expectedKind &&
                   (additionalCheck?.Invoke(tokens[checkIndex]) ?? true);
        }

        bool NextTokenIsKeyword(int index, string expectedKeyword, int next = 1)
        {
            return NextTokenIs(index, TsxTokenKind.Keyword, next, token => IsKeyword(token, expectedKeyword));
        }

        bool IsKeyword(TsxToken token, string expectedKeyword)
        {
            return token.Kind == TsxTokenKind.Keyword && token.Text == expectedKeyword;
        }

        int ScanUntil(int startIndex, TsxTokenKind[] expectedKinds, int scanUntil = -1)
        {
            var opens = new[] { TsxTokenKind.OpenParen, TsxTokenKind.OpenBrace, TsxTokenKind.OpenBracket };
            var closes = new[] { TsxTokenKind.CloseParen, TsxTokenKind.CloseBrace, TsxTokenKind.CloseBracket };
            if (scanUntil < 0)
                scanUntil = tokens.Length - 1;
            Stack<int>? nestings = null;
            for (int index = startIndex; index < scanUntil + 1; index++)
            {
                var token = tokens[index];
                if ((nestings == null || nestings.Count == 0) && 
                    expectedKinds.Contains(token.Kind))
                    return index;
                var openType = Array.IndexOf(opens, token.Kind);
                if (openType >= 0)
                {
                    nestings ??= new();
                    nestings.Push(openType);
                    continue;
                }
                var closeType = Array.IndexOf(closes, token.Kind);
                if (closeType >= 0)
                {
                    if (nestings == null || nestings.Count == 0)
                        throw new TypeScriptFunctionCallBlockParserException($"Invalid nesting, got {token.Kind} at line {token.Line}:{token.Column}");
                    if (nestings.Peek() != closeType)
                        throw new TypeScriptFunctionCallBlockParserException($"Invalid nesting, expected {closes[nestings.Peek()]}, got {token.Kind} at line {token.Line}:{token.Column}");
                    nestings.Pop();
                }
            }

            return -1;
        }

        IEnumerable<(int ParamStartIndex, int ParamEndIndex, TsxToken StartToken, TsxToken EndToken, bool IsSingleToken)> GetCommaSeparatedList(int openIndex, TsxTokenKind closingKind, int closeIndex = -1)
        {
            int elementStartIndex = openIndex + 1;
            int elementAfterIndex;

            while ((elementAfterIndex = ScanUntil(elementStartIndex, [closingKind, TsxTokenKind.Comma], closeIndex)) >= 0)
            {
                if (elementAfterIndex != elementStartIndex)
                {
                    var elementEndIndex = elementAfterIndex - 1;
                    yield return (elementStartIndex, elementEndIndex, tokens[elementStartIndex], tokens[elementEndIndex], elementEndIndex == elementStartIndex);
                }
                //empty params, like [,,], are ignored now
                if (tokens[elementAfterIndex].Kind == closingKind)
                    break;
                elementStartIndex = elementAfterIndex + 1;
            }

            if (elementAfterIndex < 0)
                throw new TypeScriptFunctionCallBlockParserException($"Could not find closing token {closingKind} for open {tokens[openIndex].Text} at line {tokens[openIndex].Line}:{tokens[openIndex].Column}");
        }

        CodeSpan GetCodeSpanBetweenTokens(TsxToken start, TsxToken end)
        {
            return new CodeSpan(codeFile, start.Start, end.End - start.Start);
        }

        string GetLiteralText(TsxToken token)
        {
            var text = token.Text;
            if (token.Kind == TsxTokenKind.StringLiteral)
                text = text.Substring(1, text.Length - 2);
            return text;
        }

        TypeScriptFunctionCallArgument.ArrayArgument GetArray(int arrayOpenIndex, int arrayCloseIndex)
        {
            var array = new TypeScriptFunctionCallArgument.ArrayArgument();

            foreach (var item in GetCommaSeparatedList(arrayOpenIndex, TsxTokenKind.CloseBracket, arrayCloseIndex))
            {
                if (item.IsSingleToken)
                {
                    array.Add(new TypeScriptFunctionCallArgument.LiteralArrayElement(GetLiteralText(item.StartToken)));
                }
                else if (item.StartToken.Kind == TsxTokenKind.OpenBracket)
                {
                    array.Add(GetArray(item.ParamStartIndex, item.ParamEndIndex));
                }
                else // multiple tokens, like for -1
                {
                    array.Add(new TypeScriptFunctionCallArgument.LiteralArrayElement(GetTokenChainText(item.ParamStartIndex, item.ParamEndIndex)));
                }
            }

            return array;
        }

        List<TypeScriptFunctionCallArgument> GetArguments(int argumentStartIndex, out int lastCallEndIndex)
        {
            lastCallEndIndex = argumentStartIndex + 1; // when no arg, the open-close param is skipped.
            var arguments = new List<TypeScriptFunctionCallArgument>();
            foreach (var callArg in GetCommaSeparatedList(argumentStartIndex, TsxTokenKind.CloseParen))
            {
                var argSpan = GetCodeSpanBetweenTokens(callArg.StartToken, tokens[callArg.ParamEndIndex]);
                var argument = new TypeScriptFunctionCallArgument(argSpan.Text, argSpan);
                if (callArg is { IsSingleToken: true, StartToken.Kind: TsxTokenKind.StringLiteral })
                {
                    argument.StringLiteral = GetLiteralText(callArg.StartToken);
                }
                else if (callArg.StartToken.Kind == TsxTokenKind.OpenBracket)
                {
                    argument.Array = GetArray(callArg.ParamStartIndex, callArg.ParamEndIndex);
                }
                else if (callArg.StartToken.Kind == TsxTokenKind.OpenParen)
                {
                    var arrowIndex = ScanUntil(callArg.ParamStartIndex, [TsxTokenKind.Arrow], callArg.ParamEndIndex);
                    if (arrowIndex >= 0 &&
                        NextTokenIs(arrowIndex, TsxTokenKind.OpenBrace))
                    {
                        argument.IsLambda = true;
                        argument.LambdaArgNames =
                            GetCommaSeparatedList(callArg.ParamStartIndex, TsxTokenKind.CloseParen, arrowIndex - 1)
                                .Select(lambdaParam => GetLiteralText(lambdaParam.StartToken))
                                .ToList();
                        argument.NestedCallBlocks = GetLambdaCallBlocks(arrowIndex + 1, callArg.ParamEndIndex).ToList();
                    }
                }
                else if (IsKeyword(callArg.StartToken, "function") &&
                         NextTokenIs(callArg.ParamStartIndex, TsxTokenKind.OpenParen))
                {
                    var openBraceIndex = ScanUntil(callArg.ParamStartIndex, [TsxTokenKind.OpenBrace], callArg.ParamEndIndex);
                    if (openBraceIndex >= 0)
                    {
                        argument.IsLambda = true;
                        argument.LambdaArgNames =
                            GetCommaSeparatedList(callArg.ParamStartIndex + 1, TsxTokenKind.CloseParen, openBraceIndex - 1)
                                .Select(lambdaParam => GetLiteralText(lambdaParam.StartToken))
                                .ToList();
                        argument.NestedCallBlocks = GetLambdaCallBlocks(openBraceIndex, callArg.ParamEndIndex).ToList();
                    }
                }

                arguments.Add(argument);
                lastCallEndIndex = callArg.ParamEndIndex + 1;
            }

            return arguments;
        }

        string GetTokenChainText(int startIndex, int endIndex)
        {
            return string.Join("",
                Enumerable.Range(startIndex, endIndex - startIndex + 1)
                    .Select(i => tokens[i].Text));
        }

        IEnumerable<TypeScriptFunctionCallBlock> GetCallBlocks(int startIndex, int endIndex = -1)
        {
            if (endIndex < 0)
                endIndex = tokens.Length - 1;
            int idSearchStartIndex = startIndex;
            int identifierIndex;
            while ((identifierIndex = Array.FindIndex(tokens, idSearchStartIndex, endIndex + 1 - idSearchStartIndex, t => t.Kind is TsxTokenKind.Identifier or TsxTokenKind.Keyword)) >= 0)
            {
                if (NextTokenIsKeyword(identifierIndex, "function", -1) ||
                    NextTokenIs(identifierIndex, TsxTokenKind.Dot, -1) ||
                    (tokens[identifierIndex].Kind == TsxTokenKind.Keyword && tokens[identifierIndex].Text != "this"))
                {
                    idSearchStartIndex++;
                    continue;
                }

                int targetStartIndex = identifierIndex;
                int targetEndIndex = identifierIndex;

                var commentTokens = tokens[identifierIndex].CommentTokens;
                var callCommentSpan = commentTokens.Any()
                    ? GetCodeSpanBetweenTokens(commentTokens.First(), commentTokens.Last())
                    : null;

                var targetArguments = new List<List<TypeScriptFunctionCallArgument>>();
                int lastProcessedIndex = targetEndIndex;
                List<TypeScriptFunctionCallArgument>? arguments = null;
                while (NextTokenIs(lastProcessedIndex, TsxTokenKind.OpenParen) ||
                       NextTokenIs(lastProcessedIndex, TsxTokenKind.TemplateLiteral) ||
                       (NextTokenIs(lastProcessedIndex, TsxTokenKind.Dot) &&
                        NextTokenIs(lastProcessedIndex, TsxTokenKind.Identifier, 2)))
                {
                    if (arguments != null)
                        targetArguments.Add(arguments);
                    arguments = null;
                    targetEndIndex = lastProcessedIndex;

                    if (NextTokenIs(lastProcessedIndex, TsxTokenKind.OpenParen))
                    {
                        try
                        {
                            arguments = GetArguments(lastProcessedIndex + 1, out lastProcessedIndex);
                        }
                        catch (TypeScriptFunctionCallBlockParserException ex)
                        {
                            errorMessages.Add(ex.Message);
                            break;
                        }
                    }
                    else if (NextTokenIs(lastProcessedIndex, TsxTokenKind.TemplateLiteral))
                    {
                        lastProcessedIndex += 1;
                        targetEndIndex = lastProcessedIndex;
                    }
                    else // .<identifier>
                    {
                        lastProcessedIndex += 2;
                        targetEndIndex = lastProcessedIndex;
                    }
                }

                idSearchStartIndex = lastProcessedIndex + 1;
                if (arguments == null) // last in the chain was not a call
                    continue;
                if (NextTokenIs(lastProcessedIndex, TsxTokenKind.OpenBrace)) // function definition
                    continue;

                var isSimpleCall = targetStartIndex == targetEndIndex;
                var functionName = isSimpleCall
                    ? tokens[targetStartIndex].Text
                    : GetTokenChainText(targetStartIndex, targetEndIndex);

                yield return new TypeScriptFunctionCallBlock(functionName, arguments.ToArray(), isSimpleCall, targetArguments.ToList(),
                    GetCodeSpanBetweenTokens(tokens[targetStartIndex], tokens[lastProcessedIndex]), callCommentSpan);
            }
        }

        IEnumerable<TypeScriptFunctionCallBlock> GetLambdaCallBlocks(int openBraceIndex, int endIndex)
        {
            if (tokens[endIndex].Kind != TsxTokenKind.CloseBrace)
                yield break;

            foreach (var callBlock in GetCallBlocks(openBraceIndex + 1, endIndex - 1))
            {
                yield return callBlock;
            }
        }

        var result = GetCallBlocks(0).ToArray();
        return new TypeScriptFunctionCallBlockParserResult(result, errorMessages.Count == 0, string.Join(Environment.NewLine, errorMessages.Distinct()), tokens.ToArray());

        //Console.WriteLine(s.ElapsedMilliseconds);

        //var inputStream = new AntlrInputStream(codeFile.SourceCode);
        //var outputWriter = new StringWriter();
        //var lexer = new TsxLexer(inputStream, outputWriter, outputWriter);
        //var tokenStream = new CommonTokenStream(lexer);
        //tokenStream.Fill();
        ////var tokens = tokenStream.GetTokens();
        //Console.WriteLine(s.ElapsedMilliseconds);
        ////Console.WriteLine(tokens.Count);
        //var parser = new TsxParser(tokenStream, outputWriter, outputWriter)
        //{
        //    BuildParseTree = true
        //};

        //var tree = parser.program();
        //Console.WriteLine(s.ElapsedMilliseconds);
        //var parserOutput = outputWriter.ToString();
        //onAstParsed?.Invoke(tree, parser, parserOutput);

        //var blockParserVisitor = new TypeScriptFunctionCallBlockParserVisitor(codeFile, tokenStream);
        //blockParserVisitor.VisitProgram(tree);

        //Console.WriteLine(s.ElapsedMilliseconds);
        //return blockParserVisitor.Result.ToArray();
    }
}