using SpecSync.Configuration;
using SpecSync.Parsing;
using SpecSync.Tracing;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.TestNGTestSource.Java;

public class JavaTestUpdater(
    EditableCodeFile codeFile,
    Func<ILocalArtifact, string> testCaseLinkTemplateProvider,
    SpecSyncConfiguration configuration,
    ISpecSyncTracer tracer)
    : CodeFileSourceDocumentUpdater(codeFile, configuration, tracer)
{
    public JavaTestUpdater(EditableCodeFile codeFile, string testCaseLinkTemplate, SpecSyncConfiguration configuration, ISpecSyncTracer tracer)
        : this(codeFile, _ => testCaseLinkTemplate, configuration, tracer)
    {
    }

    protected override string GetTagText(ILocalArtifact localArtifact, string tagName)
    {
        return testCaseLinkTemplateProvider(localArtifact).Replace("{tag}", tagName);
    }

    protected virtual string GetTestCaseLinkAnnotation(ILocalTestCase localTestCase, IdLink testCaseLink)
    {
        var tagName = GetTagName(testCaseLink);
        return $"@{GetTagText(localTestCase, tagName)}";
    }

    public override void SetArtifactLink(ILocalArtifact localArtifact, IdLink idLink)
    {
        SetDirty();
        var methodLocalTestCase = (JavaTestMethodLocalTestCase)localArtifact;
        SetTestCaseLinkInternal(methodLocalTestCase, idLink);
    }

    protected virtual void SetTestCaseLinkInternal(JavaTestMethodLocalTestCase methodLocalTestCase, IdLink testCaseLink)
    {
        var annotation = GetTestCaseLinkAnnotation(methodLocalTestCase, testCaseLink);
        var lastMetadata = methodLocalTestCase.MethodBlock.Metadata.LastOrDefault();
        if (lastMetadata != null)
        {
            CodeFile.Updater.InsertLineAfter(lastMetadata.End, annotation);
        }
        else
        {
            CodeFile.Updater.InsertLineBefore(methodLocalTestCase.MethodBlock.SourceSpan.Start, annotation);
        }
    }

    protected override CodeSpan ExtendTagSpanToDelete(CodeSpan tagSourceSpan)
    {
        bool IsInlineWhitespace(char c) => c == ' ' || c == '\t';
        bool IsNewLine(char c) => c == '\r' || c == '\n';
        var resultSpan = CodeFile.ExtendSpan(tagSourceSpan,
            c => c == ',' || IsInlineWhitespace(c),
            c => c == ',' || IsInlineWhitespace(c));
        if (resultSpan.Text.Trim().EndsWith(","))
        {
            resultSpan = new CodeSpan(CodeFile, tagSourceSpan.Start, resultSpan.End);
        }
        else if (resultSpan.Text.Trim().StartsWith(","))
        {
            resultSpan = new CodeSpan(CodeFile, resultSpan.Start, tagSourceSpan.End);
        }
        else if (resultSpan.Length > 0 && IsInlineWhitespace(resultSpan.Text[0]) && IsInlineWhitespace(resultSpan.Text.Last()))
        {
            resultSpan = CodeFile.ShrinkSpan(resultSpan, IsInlineWhitespace, null);
        }

        if (CodeFile.IsAtLineStart(resultSpan.Start) && CodeFile.IsAtLineEnd(resultSpan.End))
        {
            resultSpan = CodeFile.ExtendSpan(resultSpan, null, IsNewLine);
        }
        return resultSpan;
    }
}