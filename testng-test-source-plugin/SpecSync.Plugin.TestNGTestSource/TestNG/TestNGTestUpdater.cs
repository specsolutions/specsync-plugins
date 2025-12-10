using SpecSync.Configuration;
using SpecSync.Parsing;
using SpecSync.Plugin.TestNGTestSource.Java;
using SpecSync.Tracing;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.TestNGTestSource.TestNG;

public class TestNGTestUpdater(EditableCodeFile codeFile, SpecSyncConfiguration configuration, ISpecSyncTracer tracer)
    : JavaTestUpdater(codeFile, @"""{tag}""", configuration, tracer)
{
    protected override void SetTestCaseLinkInternal(JavaTestMethodLocalTestCase methodLocalTestCase, IdLink testCaseLink)
    {
        var testCase = (TestNGTestMethodLocalTestCase)methodLocalTestCase;
        if (testCase.MethodTestAnnotation == null)
            base.SetTestCaseLinkInternal(methodLocalTestCase, testCaseLink);
        else
        {
            var tagName = GetTagName(testCaseLink);
            var quotedTagName = @$"""{tagName}""";
            var groupsParam = $"{TestNGTestClassParser.GroupElementName} = {{ {quotedTagName} }}";

            var groupsElement = testCase.MethodTestAnnotation.Elements
                .FirstOrDefault(e => e.Name == TestNGTestClassParser.GroupElementName);
            if (groupsElement == null)
            {
                if (testCase.MethodTestAnnotation.ElementsSpan == null)
                {
                    CodeFile.Updater.Insert(testCase.MethodTestAnnotation.End, "(" + groupsParam + ")");
                }
                else if (testCase.MethodTestAnnotation.Elements.Length == 0)
                {
                    CodeFile.Updater.Insert(testCase.MethodTestAnnotation.ElementsSpan.Start, groupsParam);
                }
                else
                {
                    CodeFile.Updater.Insert(testCase.MethodTestAnnotation.ElementsSpan.Start, groupsParam + ", ");
                }
            }
            else
            {
                var valueSpanText = groupsElement.ValueSpan.Text;
                var curlyOpenIndex = valueSpanText.IndexOf('{');
                var curlyCloseIndex = valueSpanText.IndexOf('}');
                if (curlyOpenIndex < 0 || curlyCloseIndex < 0 || curlyCloseIndex < curlyOpenIndex)
                {
                    CodeFile.Updater.Replace(groupsElement.ValueSpan, "{ " + quotedTagName + " }");
                }
                else
                {
                    var startPosition = curlyOpenIndex + 1;
                    while (startPosition < valueSpanText.Length && char.IsWhiteSpace(valueSpanText[startPosition]))
                    {
                        startPosition++;
                    }

                    var isEmpty = valueSpanText[startPosition] == '}';
                    var insertPosition = CodeFile.GetPosition(CodeFile.GetSourceCodeIndex(groupsElement.ValueSpan.Start) + startPosition);
                    CodeFile.Updater.Insert(insertPosition, quotedTagName + (isEmpty ? "" : ", "));
                }
            }
        }
    }

    protected override string GetTestCaseLinkAnnotation(ILocalTestCase localTestCase, IdLink testCaseLink)
    {
        var tagName = GetTagName(testCaseLink);
        return $"@Test(groups = {{ {GetTagText(localTestCase, tagName)} }})";
    }
}