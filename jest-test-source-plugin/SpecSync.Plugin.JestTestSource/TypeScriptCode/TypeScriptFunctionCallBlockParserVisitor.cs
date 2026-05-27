using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using SpecSync.PluginDependency.TypeScriptSource.TypeScriptCode.TsxGrammar;
using SpecSync.Utils;
using SpecSync.Utils.Code;
using static SpecSync.Plugin.JestTestSource.TypeScriptCode.TypeScriptFunctionCallArgument;
using static SpecSync.PluginDependency.TypeScriptSource.TypeScriptCode.TsxGrammar.TsxParser;

namespace SpecSync.Plugin.JestTestSource.TypeScriptCode;

internal class TypeScriptFunctionCallBlockParserVisitor(CodeFile codeFile, CommonTokenStream tokenStream)
    : TsxParserBaseVisitor<object>
{
    class CallContext
    {
        public string? FunctionTarget { get; set; }
        public List<TypeScriptFunctionCallArgument>? CallArguments => CallArgumentsList.FirstOrDefault();
        public List<List<TypeScriptFunctionCallArgument>> CallArgumentsList { get; } = new();
        public IToken[]? Comments { get; set; }
        public bool IsSimpleCall { get; set; }
    }

    private readonly Stack<CallContext> _callContextStack = new();
    private readonly Stack<List<TypeScriptFunctionCallArgument>> _argumentListStack = new();
    private readonly Stack<ArrayArgument> _arrayStack = new();
    private bool _inCallTarget = false;
    private ArrayArgument? _lastArray = null;
    private readonly List<string> _lambdaArgNames = new();

    public List<TypeScriptFunctionCallBlock> Result { get; } = new();

    private CodeSpan GetCodeSpan(ParserRuleContext context)
        => new(codeFile, context.Start.StartIndex, context.Stop.StopIndex - context.Start.StartIndex + 1);

    private CodeSpan GetCodeSpan(IToken token)
        => new(codeFile, token.StartIndex, token.StopIndex - token.StartIndex + 1);

    private void VisitCallInternal(ParserRuleContext context, ParserRuleContext targetContext, Action invokeArguments, IdentifierOrKeyWordContext? preTarget = null)
    {
        var callContext = new CallContext();
        _callContextStack.Push(callContext);

        var comments = tokenStream
            .GetHiddenTokensToLeft(context.Start.TokenIndex)?
            .Where(t => t.Type == TsxLexer.SingleLineComment || t.Type == TsxLexer.MultiLineComment)
            .ToArray();
        if (comments != null && comments.Any())
            callContext.Comments = comments;

        callContext.FunctionTarget = targetContext.GetText();
        if (preTarget != null)
            callContext.FunctionTarget = $"{preTarget.GetText()}{callContext.FunctionTarget}";
        if (preTarget == null && targetContext is IdentifierOrKeyWordContext)
            callContext.IsSimpleCall = true;
        _inCallTarget = true;
        var targetCallContext = new CallContext();
        _callContextStack.Push(targetCallContext);
        if (preTarget != null)
            preTarget.Accept(this);
        targetContext.Accept(this);
        _callContextStack.Pop();
        _inCallTarget = false;

        invokeArguments();

        if (_callContextStack.Peek() == callContext)
        {
            _callContextStack.Pop();
            var sourceSpan = GetCodeSpan(context);

            var commentSpan = callContext.Comments != null && callContext.Comments.Any()
                ? new CodeSpan(codeFile, GetCodeSpan(callContext.Comments.First()).Start,
                    GetCodeSpan(callContext.Comments.Last()).End)
                : null;

            var callBlock = new TypeScriptFunctionCallBlock(callContext.FunctionTarget ?? "", callContext.CallArguments?.ToArray() ?? [], callContext.IsSimpleCall, targetCallContext.CallArgumentsList, sourceSpan, commentSpan);

            if (_argumentListStack.Any() && _argumentListStack.Peek().Any())
                _argumentListStack.Peek().Last().AddNestedCall(callBlock);
            else
                Result.Add(callBlock);
        }
    }

    private void VisitArgumentListInternal(IEnumerable<ParserRuleContext> arguments)
    {
        var callArguments = new List<TypeScriptFunctionCallArgument>();
        _argumentListStack.Push(callArguments);
        if (_callContextStack.Any())
            _callContextStack.Peek().CallArgumentsList.Add(callArguments);

        foreach (var arg in arguments)
        {
            var callArgument = new TypeScriptFunctionCallArgument(arg.GetText(), GetCodeSpan(arg));
            callArguments.Add(callArgument);

            if (arg is LiteralExpressionContext literalExpression &&
                literalExpression.GetChild(0) is LiteralContext literal &&
                literal.StringLiteral() != null)
            {
                var text = literal.StringLiteral().GetText();
                callArgument.StringLiteral = text.Substring(1, text.Length - 2);
            }
            if (arg is FunctionExpressionContext functionExpression &&
                functionExpression.GetChild(0) is AnonymousFunctionContext)
            {
                callArgument.IsLambda = true;
                VisitChildren(arg);
                callArgument.LambdaArgNames ??= new();
            }

            if (arg is ArrayLiteralExpressionContext arrayLiteralExpression &&
                arrayLiteralExpression.GetChild(0) is ArrayLiteralContext arrayLiteral)
            {
                _lastArray = null;
                arrayLiteral.Accept(this);
                if (_lastArray != null)
                    callArgument.Array = _lastArray;
            }
        }

        if (_argumentListStack.Pop() != callArguments)
            throw new SpecSyncException("Invalid call nesting");
    }

    // variableDeclaration: <identifierOrKeyWord> <singleExpression:ParenthesizedExpression>
    // matches to top-level method calls, like foo(arg1, arg2)
    public override object VisitVariableDeclaration(VariableDeclarationContext context)
    {
        if (!_inCallTarget &&
            context.GetChild(0) is IdentifierOrKeyWordContext targetContext &&
            context.GetChild(1) is ParenthesizedExpressionContext arguments)
        {
            VisitCallInternal(context, targetContext, () => VisitParenthesizedExpression(arguments));
        }
        else if (!_inCallTarget && 
                 context.GetChild(0) is IdentifierOrKeyWordContext outerFunction &&
                 context.GetChild(1) is ArgumentsExpressionContext functionCallReminder)
        {
            // hack: the parser incorrectly recognizes the expressions in case of a(x).b()
            VisitCallInternal(context, functionCallReminder.singleExpression(), () => VisitArguments(functionCallReminder.arguments()), preTarget: outerFunction);
        }
        else
        {
            base.VisitVariableDeclaration(context);
        }

        return DefaultResult;
    }

    private bool IsReturn(IdentifierNameContext targetContext)
    {
        return targetContext.GetChild(0) is ReservedWordContext reservedWordContext &&
               reservedWordContext.GetChild(0) is KeywordContext keywordContext &&
               keywordContext.GetChild(0) is ITerminalNode terminal &&
               terminal.Symbol.Type == TsxLexer.Return;
    }

    public override object VisitIdentifierExpression(IdentifierExpressionContext context)
    {
        if (!_inCallTarget &&
            context.GetChild(0) is IdentifierNameContext targetContext &&
                !IsReturn(targetContext) &&
            context.GetChild(1) is ParenthesizedExpressionContext arguments)
        {
            VisitCallInternal(context, targetContext, () => VisitParenthesizedExpression(arguments));
        }
        else
        {
            base.VisitIdentifierExpression(context);
        }

        return DefaultResult;
    }

    // ArgumentsExpression: <singleExpression:<MemberDotExpression:(<singleExpression:identifierName>.<identifierName>)>> <arguments:argumentList>
    public override object VisitArgumentsExpression(ArgumentsExpressionContext context)
    {
        if (!_inCallTarget && 
            context.GetChild(0) is SingleExpressionContext singleExpression &&
            context.GetChild(1) is ArgumentsContext arguments)
        {
            VisitCallInternal(context, singleExpression, () => VisitArguments(arguments));
        }
        else
        {
            base.VisitArgumentsExpression(context);
        }

        return DefaultResult;
    }


    // matches call parameters of top-level method calls as a children of <variableDeclaration>
    public override object VisitParenthesizedExpression(ParenthesizedExpressionContext context)
    {
        VisitArgumentListInternal(context.children.OfType<ExpressionSequenceContext>().First().children.OfType<SingleExpressionContext>());
        return DefaultResult;
    }

    public override object VisitArguments(ArgumentsContext context)
    {
        VisitArgumentListInternal(context.argumentList()?.children.OfType<ArgumentContext>().Select(arg => arg.children.OfType<ParserRuleContext>().FirstOrDefault()).Where(a => a != null) ?? []);
        return DefaultResult;
    }

    public override object VisitArrayLiteral(ArrayLiteralContext context)
    {
        var array = new ArrayArgument();
        _arrayStack.Push(array);
        base.VisitArrayLiteral(context);
        _arrayStack.Pop();
        _lastArray = array;
        if (_arrayStack.Any())
        {
            var currentArray = _arrayStack.Peek();
            if (currentArray.Any())
                currentArray[currentArray.Count - 1] = array;
        }

        return DefaultResult;
    }

    public override object VisitArrayElement(ArrayElementContext context)
    {
        if (_arrayStack.Any())
            _arrayStack.Peek().Add(new LiteralArrayElement(context.GetText()));
        base.VisitArrayElement(context);
        return DefaultResult;
    }

    public override object VisitFormalParameterList(FormalParameterListContext context)
    {
        _lambdaArgNames.Clear();
        base.VisitFormalParameterList(context);
        if (_argumentListStack.Any() && _argumentListStack.Peek().Any() &&
            _argumentListStack.Peek().Last().LambdaArgNames == null)
            _argumentListStack.Peek().Last().LambdaArgNames = _lambdaArgNames.ToList();
        return DefaultResult;
    }

    public override object VisitIdentifier(IdentifierContext context)
    {
        if (MatchParents(context, typeof(AssignableContext), typeof(FormalParameterArgContext)))
        {
            _lambdaArgNames.Add(context.Identifier().GetText());
        }

        return base.VisitIdentifier(context);
    }

    private bool MatchParents(RuleContext context, params Type?[] parentTypes)
    {
        foreach (var parentType in parentTypes)
        {
            if (context == null)
                return false;
            if (parentType != null && 
                (context.Parent == null || !parentType.IsInstanceOfType(context.Parent)))
                return false;
            context = context.Parent;
        }
        return true;
    }
}