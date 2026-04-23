using SpecSync.Parsing;
using SpecSync.Plugin.JestTestSource.TypeScriptCode;
using SpecSync.TestMethodSource;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.JestTestSource.Jest;

public class JestTestLocalTestCase(
    string name,
    string originalTitle,
    string[] originalAncestorTitles,
    IEnumerable<ILocalArtifactTag> tags,
    IdLink? testCaseLink,
    TypeScriptFunctionCallBlock methodBlock,
    CodeSpan testTextSpan,
    LocalTestCaseDataRow[]? dataRows = null,
    string[]? parameterNames = null,
    AcceptanceCriterion? testedRule = null,
    string? description = null)
    : TestMethodLocalTestCase(name, null, JestJsonResultLoader.GetClassName(originalAncestorTitles), originalTitle, tags, testCaseLink, methodBlock.SourceSpan, dataRows, description, testedRule, 2, parameterNames)
{
    public CodeSpan TestTextSpan { get; } = testTextSpan;
    public string InvokedFunction { get; } = methodBlock.FunctionName;
}