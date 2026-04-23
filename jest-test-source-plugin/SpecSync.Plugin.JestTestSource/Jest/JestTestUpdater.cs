using SpecSync.Configuration;
using SpecSync.Parsing;
using SpecSync.Tracing;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.JestTestSource.Jest;

public class JestTestUpdater(EditableCodeFile codeFile, SpecSyncConfiguration configuration, ISpecSyncTracer tracer)
    : CodeFileSourceDocumentUpdater(codeFile, configuration, tracer)
{
    public override void SetArtifactLink(ILocalArtifact localArtifact, IdLink idLink)
    {
        SetDirty();
        var methodLocalTestCase = (JestTestLocalTestCase)localArtifact;
        SetTestCaseLinkInternal(methodLocalTestCase, idLink);
    }

    protected virtual void SetTestCaseLinkInternal(JestTestLocalTestCase jestLocalTestCase, IdLink testCaseLink)
    {
        var originalText = jestLocalTestCase.TestTextSpan.Text;
        var newTag = GetTagText(jestLocalTestCase, $"{testCaseLink.LinkPrefix}:{testCaseLink.Id}");
        var tagsBlockMatch = JestTestClassParser.TagsRe.Match(originalText);
        string updatedText;
        if (tagsBlockMatch.Success)
        {
            var tagsBlockContentGroup = tagsBlockMatch.Groups["tagsBlockContent"];
            var prefix = originalText.Substring(0, tagsBlockContentGroup.Index);
            var remainingTags = tagsBlockContentGroup.Value;
            var postfix = originalText.Substring(prefix.Length + remainingTags.Length);
            remainingTags = remainingTags.TrimStart();
            if (remainingTags.Length > 0)
                remainingTags = " " + remainingTags;
            updatedText = prefix + newTag + remainingTags + postfix;
        }
        else
        {
            updatedText = originalText + $" [{newTag}]";
        }

        CodeFile.Updater.Replace(jestLocalTestCase.TestTextSpan, updatedText);
    }

    protected override string GetTagText(ILocalArtifact localArtifact, string tagName)
    {
        return "@" + tagName;
    }

    protected override CodeSpan ExtendTagSpanToDelete(CodeSpan tagSourceSpan)
    {
        var spanToDelete = CodeFile.ExtendSpan(tagSourceSpan, null, Utils.Code.CodeFile.IsInlineWhitespace);
        if (CodeFile.ExtendSpan(spanToDelete, null, c => c == ']').Length > spanToDelete.Length) // if at the end, we remove whitespace from left
            spanToDelete = CodeFile.ExtendSpan(tagSourceSpan, Utils.Code.CodeFile.IsInlineWhitespace, null);
        return spanToDelete;
    }
}