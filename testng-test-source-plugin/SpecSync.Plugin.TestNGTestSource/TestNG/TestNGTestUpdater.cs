using System;
using System.Linq;
using SpecSync.Configuration;
using SpecSync.Parsing;
using SpecSync.Plugin.TestNGTestSource.Java;
using SpecSync.Tracing;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.TestNGTestSource.TestNG;

public class TestNGTestUpdater : JavaTestUpdater
{
    public TestNGTestUpdater(EditableCodeFile codeFile, SpecSyncConfiguration configuration, ISpecSyncTracer tracer) 
        : base(codeFile, @"""{tag}""", configuration, tracer)
    {
    }

    protected override void SetTestCaseLinkInternal(JavaTestMethodLocalTestCase methodLocalTestCase, TestCaseLink testCaseLink)
    {
        var testCase = (TestNGTestMethodLocalTestCase)methodLocalTestCase;
        if (testCase.MethodTestAnnotation == null)
            base.SetTestCaseLinkInternal(methodLocalTestCase, testCaseLink);
        else
        {
            var tagName = GetTagName(testCaseLink);
            var quotedTagName = @$"""{tagName}""";
            var groupsParam = $@"{TestNGTestClassParser.GroupElementName} = {{ {quotedTagName} }}";

            var groupsElement = testCase.MethodTestAnnotation.Elements
                .FirstOrDefault(e => e.Name == TestNGTestClassParser.GroupElementName);
            if (groupsElement == null)
            {
                if (testCase.MethodTestAnnotation.ElementsSpan == null)
                {
                    _codeFile.Updater.Insert(testCase.MethodTestAnnotation.End, "(" + groupsParam + ")");
                }
                else if (testCase.MethodTestAnnotation.Elements.Length == 0)
                {
                    _codeFile.Updater.Insert(testCase.MethodTestAnnotation.ElementsSpan.Start, groupsParam);
                }
                else
                {
                    _codeFile.Updater.Insert(testCase.MethodTestAnnotation.ElementsSpan.Start, groupsParam + ", ");
                }
            }
            else
            {
                var valueSpanText = groupsElement.ValueSpan.Text;
                var curlyOpenIndex = valueSpanText.IndexOf('{');
                var curlyCloseIndex = valueSpanText.IndexOf('}');
                if (curlyOpenIndex < 0 || curlyCloseIndex < 0 || curlyCloseIndex < curlyOpenIndex)
                {
                    _codeFile.Updater.Replace(groupsElement.ValueSpan, "{ " + quotedTagName + " }");
                }
                else
                {
                    var startPosition = curlyOpenIndex + 1;
                    while (startPosition < valueSpanText.Length && char.IsWhiteSpace(valueSpanText[startPosition]))
                    {
                        startPosition++;
                    }

                    var isEmpty = valueSpanText[startPosition] == '}';
                    var insertPosition = _codeFile.GetPosition(_codeFile.GetSourceCodeIndex(groupsElement.ValueSpan.Start) + startPosition);
                    _codeFile.Updater.Insert(insertPosition, quotedTagName + (isEmpty ? "" : ", "));
                }
            }
        }
    }

    protected override string GetTestCaseLinkAnnotation(ILocalTestCase localTestCase, TestCaseLink testCaseLink)
    {
        var tagName = GetTagName(testCaseLink);
        return $"@Test(groups = {{ {GetTagText(localTestCase, tagName)} }})";
    }
}