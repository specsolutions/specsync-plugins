using SpecSync.Configuration;
using SpecSync.Parsing;
using SpecSync.Tracing;
using SpecSync.Utils.Code;
using System;
using System.Linq;

namespace SpecSync.Plugin.TestNGTestSource.Java;

public class JavaTestUpdater : CodeFileLocalTestCaseUpdater
{
    private readonly Func<ILocalTestCase, string> _testCaseLinkTemplateProvider;

    public JavaTestUpdater(EditableCodeFile codeFile, string testCaseLinkTemplate, SpecSyncConfiguration configuration, ISpecSyncTracer tracer)
        : this(codeFile, _ => testCaseLinkTemplate, configuration, tracer)
    {
    }

    public JavaTestUpdater(EditableCodeFile codeFile, Func<ILocalTestCase, string> testCaseLinkTemplateProvider, SpecSyncConfiguration configuration, ISpecSyncTracer tracer)
        : base(codeFile, configuration, tracer)
    {
        _testCaseLinkTemplateProvider = testCaseLinkTemplateProvider;
    }


    protected override string GetTagText(ILocalTestCase localTestCase, string tagName)
    {
        return _testCaseLinkTemplateProvider(localTestCase).Replace("{tag}", tagName);
    }

    protected virtual string GetTestCaseLinkAnnotation(ILocalTestCase localTestCase, TestCaseLink testCaseLink)
    {
        var tagName = GetTagName(testCaseLink);
        return $"@{GetTagText(localTestCase, tagName)}";
    }

    public override void SetTestCaseLink(ILocalTestCase localTestCase, TestCaseLink testCaseLink)
    {
        SetDirty();
        var methodLocalTestCase = (JavaTestMethodLocalTestCase)localTestCase;
        SetTestCaseLinkInternal(methodLocalTestCase, testCaseLink);
    }

    protected virtual void SetTestCaseLinkInternal(JavaTestMethodLocalTestCase methodLocalTestCase, TestCaseLink testCaseLink)
    {
        var annotation = GetTestCaseLinkAnnotation(methodLocalTestCase, testCaseLink);
        var lastMetadata = methodLocalTestCase.MethodBlock.Metadata.LastOrDefault();
        if (lastMetadata != null)
        {
            _codeFile.Updater.InsertLineAfter(lastMetadata.End, annotation);
        }
        else
        {
            _codeFile.Updater.InsertLineBefore(methodLocalTestCase.MethodBlock.SourceSpan.Start, annotation);
        }
    }

    protected override CodeSpan ExtendTagSpanToDelete(CodeSpan tagSourceSpan)
    {
        bool IsInlineWhitespace(char c) => c == ' ' || c == '\t';
        bool IsNewLine(char c) => c == '\r' || c == '\n';
        var resultSpan = _codeFile.ExtendSpan(tagSourceSpan,
            c => c == ',' || IsInlineWhitespace(c),
            c => c == ',' || IsInlineWhitespace(c));
        if (resultSpan.Text.Trim().EndsWith(","))
        {
            resultSpan = new CodeSpan(_codeFile, tagSourceSpan.Start, resultSpan.End);
        }
        else if (resultSpan.Text.Trim().StartsWith(","))
        {
            resultSpan = new CodeSpan(_codeFile, resultSpan.Start, tagSourceSpan.End);
        }
        else if (resultSpan.Length > 0 && IsInlineWhitespace(resultSpan.Text[0]) && IsInlineWhitespace(resultSpan.Text.Last()))
        {
            resultSpan = _codeFile.ShrinkSpan(resultSpan, IsInlineWhitespace, null);
        }

        if (_codeFile.IsAtLineStart(resultSpan.Start) && _codeFile.IsAtLineEnd(resultSpan.End))
        {
            resultSpan = _codeFile.ExtendSpan(resultSpan, null, IsNewLine);
        }
        return resultSpan;
    }
}