using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using JavaParserPlay.JavaCode.JavaGrammar;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.TestNGTestSource.JavaCode;

internal class JavaMethodBlockParserVisitor : JavaParserBaseVisitor<object>
{
    private readonly CodeFile _codeFile;
    private readonly IList<IToken> _tokens;

    private string _packageName = null;
    private readonly Stack<string> _classNames = new();
    private string _methodName = null;
    private readonly List<string> _methodParameters = new();
    private string _annotationName = null;
    private readonly List<JavaAnnotation> _methodAnnotations = new();
    private readonly List<JavaAnnotation> _classAnnotations = new();
    private List<JavaAnnotation> _annotationTarget;
    private readonly List<JavaAnnotationElement> _annotationElements = new();

    public List<JavaMethodBlock> Result { get; } = new();

    public JavaMethodBlockParserVisitor(CodeFile codeFile, IList<IToken> tokens)
    {
        _codeFile = codeFile;
        _tokens = tokens;
        _annotationTarget = _classAnnotations;
    }

    private CodeSpan GetCodeSpan(ParserRuleContext context)
        => new(_codeFile, context.Start.StartIndex, context.Stop.StopIndex - context.Start.StartIndex + 1);

    private CodeSpan GetCodeSpan(IToken token)
        => new(_codeFile, token.StartIndex, token.StopIndex - token.StartIndex + 1);

    public override object VisitClassBodyDeclaration(JavaParser.ClassBodyDeclarationContext context)
    {
        base.VisitClassBodyDeclaration(context);

        if (_methodName != null)
        {
            var className = string.Join(".", _classNames.Reverse());
            var sourceSpan = new CodeSpan(_codeFile, 
                new CodePosition(context.Start.Line-1, context.Start.Column), 
                context.Stop.StopIndex - context.Start.StartIndex + 1);

            var docCommentSpan = FindDocCommentSpan(context.Start.TokenIndex, -1) 
                ?? FindDocCommentSpanBetweenModifiers(context);

            if (docCommentSpan != null && docCommentSpan.Start.CompareTo(sourceSpan.Start) < 0)
                sourceSpan = new CodeSpan(_codeFile, docCommentSpan.Start, sourceSpan.End);

            if (_codeFile.IsAtLineStart(sourceSpan.Start))
                sourceSpan = new CodeSpan(_codeFile, new CodePosition(sourceSpan.StartLine, 0), sourceSpan.End);

            var javaMethodBlock = new JavaMethodBlock(_packageName, className, _methodName, _methodParameters.ToArray(),
                _methodAnnotations.ToArray(), _classAnnotations.ToArray(), sourceSpan, docCommentSpan);
            Result.Add(javaMethodBlock);
        }

        _methodAnnotations.Clear();
        _methodName = null;
        _methodParameters.Clear();

        return null;
    }

    private CodeSpan FindDocCommentSpanBetweenModifiers(JavaParser.ClassBodyDeclarationContext context)
    {
        var modifiers = context.children.OfType<JavaParser.ModifierContext>().ToArray();

        for (int annotationIndex = 0; annotationIndex < modifiers.Length; annotationIndex++)
        {
            var searchFrom = annotationIndex == modifiers.Length - 1
                ? context.Start.TokenIndex
                : modifiers[annotationIndex + 1].Start.TokenIndex;
            var docCommentSpan = FindDocCommentSpan(searchFrom, modifiers[annotationIndex].Stop.TokenIndex);
            if (docCommentSpan != null)
                return docCommentSpan;
        }

        return null;
    }

    private CodeSpan FindDocCommentSpan(int searchFrom, int searchUntil)
    {
        var docCommentTokenIndex = searchFrom - 1;
        while (docCommentTokenIndex > searchUntil)
        {
            var token = _tokens[docCommentTokenIndex];
            if (token.Type == JavaLexer.COMMENT)
            {
                return GetCodeSpan(token);
            }
            if (!new[] { JavaLexer.LINE_COMMENT, JavaLexer.WS }.Contains(token.Type))
            {
                break;
            }

            docCommentTokenIndex--;
        }

        return null;
    }

    public override object VisitIdentifier(JavaParser.IdentifierContext context)
    {
        if (MatchParents(context, typeof(JavaParser.ClassDeclarationContext)))
        {
            var identifier = context.GetText();
            _classNames.Push(identifier);
        }
        else if (MatchParents(context, typeof(JavaParser.MethodDeclarationContext)))
            _methodName = context.GetText();
        else if (MatchParents(context, null, typeof(JavaParser.FormalParameterContext), null, typeof(JavaParser.FormalParametersContext), typeof(JavaParser.MethodDeclarationContext)))
            _methodParameters.Add(context.GetText());

        return base.VisitIdentifier(context);
    }

    private bool MatchParents(RuleContext context, params Type[] parentTypes)
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

    public override object VisitQualifiedName(JavaParser.QualifiedNameContext context)
    {
        if (context.Parent is JavaParser.PackageDeclarationContext)
            _packageName = context.GetText();
        else if (context.Parent is JavaParser.AnnotationContext)
            _annotationName = context.GetText();
        return base.VisitQualifiedName(context);
    }

    private ITerminalNode GetTerminalNode(ParserRuleContext context, int index, int nodeType)
    {
        var node = context.GetChild<TerminalNodeImpl>(index);
        if (node == null || node.Symbol.Type != nodeType)
            return null;
        return node;
    }

    public override object VisitAnnotation(JavaParser.AnnotationContext context)
    {
        _annotationName = null;
        base.VisitAnnotation(context);
        var elementsOpen = GetTerminalNode(context, 1, JavaLexer.LPAREN);
        var elementsClose = GetTerminalNode(context, 2, JavaLexer.RPAREN);
        var elementsSpan = elementsOpen == null || elementsClose == null
            ? null
            : new CodeSpan(_codeFile, GetCodeSpan(elementsOpen.Symbol).End, GetCodeSpan(elementsClose.Symbol).Start);
        var javaAnnotation = new JavaAnnotation(_codeFile, context.Start.StartIndex, context.Stop.StopIndex - context.Start.StartIndex + 1, _annotationName, _annotationElements.ToArray(), elementsSpan);
        _annotationTarget.Add(javaAnnotation);
        _annotationElements.Clear();
        return null;
    }

    public override object VisitClassBody(JavaParser.ClassBodyContext context)
    {
        _annotationTarget = _methodAnnotations;
        return base.VisitClassBody(context);
    }

    public override object VisitClassDeclaration(JavaParser.ClassDeclarationContext context)
    {
        var visitClassDeclaration = base.VisitClassDeclaration(context);
        if (_classNames.Count > 0)
            _classNames.Pop();
        return visitClassDeclaration;
    }

    public override object VisitElementValue(JavaParser.ElementValueContext context)
    {
        var elementValue = GetElementValue(context);
        var valueSpan = GetCodeSpan(context);
        if (context.Parent is JavaParser.ElementValuePairContext elementValuePairContext)
        {
            var name = elementValuePairContext.GetChild(0).GetText();
            _annotationElements.Add(new JavaAnnotationElement(name, elementValue, valueSpan));
        }
        else
        {
            _annotationElements.Add(new JavaAnnotationElement(elementValue, valueSpan));
        }

        return null;
    }

    private object GetElementValue(JavaParser.ElementValueContext context)
    {
        if (context.GetChild(0) is JavaParser.ElementValueArrayInitializerContext elementValueArrayInitializerContext)
        {
            var arrayElements = elementValueArrayInitializerContext.children.OfType<JavaParser.ElementValueContext>().ToArray();
            return arrayElements.Select(itCtx => new JavaAnnotationElement(GetElementValue(itCtx), GetCodeSpan(itCtx))).ToArray();
        }

        var valueText = context.GetText();
        Match match;
        if ((match = Regex.Match(valueText, @"^""(?<value>.*)""$")).Success)
            return match.Groups["value"].Value;
        if ((match = Regex.Match(valueText, @"^'(?<value>.+)'$")).Success)
            return match.Groups["value"].Value;
        return valueText;
    }
}