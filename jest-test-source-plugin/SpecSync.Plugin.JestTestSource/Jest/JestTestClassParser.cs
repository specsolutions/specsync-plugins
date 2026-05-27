using SpecSync.Parsing;
using SpecSync.Plugin.JestTestSource.TypeScript;
using SpecSync.Plugin.JestTestSource.TypeScriptCode;
using SpecSync.TestMethodSource;
using SpecSync.Utils.Code;
using System.Text.RegularExpressions;

namespace SpecSync.Plugin.JestTestSource.Jest;

public class JestTestClassParser : TypeScriptTestFunctionParserBase
{
    public static readonly string[] DescribeExtensions =
    [
        "each",
        "only",
        "only.each",
        "skip",
        "skip.each",
    ];

    public static readonly string[] TestExtensions =
    [
        "concurrent",
        "failing",
        "only.failing",
        "skip.failing",
        "only",
        "skip",
        "todo",
    ];

    public static readonly string[] ParametrizedTestExtensions =
    [
        "each",
        "concurrent.each",
        "concurrent.only.each",
        "concurrent.skip.each",
        "failing.each",
        "only.each",
        "skip.each",
    ];

    public static readonly Regex TagsRe = new(@"\[(?<tagsBlockContent>\s*(?<tagList>[^\]]*)\s*)\]");

    public override string ServiceDescription => "Jest test class parser";

    protected internal override IEnumerable<TestMethodLocalTestCase> GetTestMethodLocalTestCases(SourceDocumentParserArgs args, EditableCodeFile codeFile)
    {
        var callBlocks = ParseCallBlocks(codeFile, args);

        (string Title, string OriginalTitle)[] ancestors = [];
        CodeFileLocalArtifactTag[] ancestorTags = [];

        foreach (var jestTestLocalTestCase in ProcessCallBlocks(callBlocks, ancestors, ancestorTags, codeFile, args))
            yield return jestTestLocalTestCase;
    }

    private IEnumerable<JestTestLocalTestCase> ProcessCallBlocks(TypeScriptFunctionCallBlock[] callBlocks,
        (string Title, string OriginalTitle)[] ancestors, CodeFileLocalArtifactTag[] ancestorTags,
        EditableCodeFile codeFile, SourceDocumentParserArgs args)
    {
        foreach (var call in callBlocks)
        {
            if (IsDescribe(call))
            {
                foreach (var localTestCase in ProcessDescribeCall(call, ancestors, ancestorTags, codeFile, args))
                    yield return localTestCase;
            }
            else if (IsTest(call, out var parametrized))
            {
                var localTestCase = ProcessTestCall(call, parametrized, ancestors, ancestorTags, codeFile, args);
                if (localTestCase != null)
                    yield return localTestCase;
            }
        }
    }

    private IEnumerable<JestTestLocalTestCase> ProcessDescribeCall(TypeScriptFunctionCallBlock call, (string Title, string OriginalTitle)[] ancestors, CodeFileLocalArtifactTag[] ancestorTags, EditableCodeFile codeFile, SourceDocumentParserArgs args)
    {
        var describeTitleArg = GetDescribeTitle(call);
        if (describeTitleArg == null)
            yield break;
        var describeBody = GetDescribeBody(call);
        if (describeBody == null)
            yield break;

        var describeTitle = ParseText(describeTitleArg, out var describeTags, out _, out var originalDescribeTitle, codeFile, args);

        ancestors = [..ancestors, (describeTitle, originalDescribeTitle)];
        ancestorTags = [..ancestorTags, ..describeTags];

        foreach (var jestTestLocalTestCase in ProcessCallBlocks(describeBody, ancestors, ancestorTags, codeFile, args)) 
            yield return jestTestLocalTestCase;
    }

    private JestTestLocalTestCase? ProcessTestCall(TypeScriptFunctionCallBlock call, bool isParametrized,
        (string Title, string OriginalTitle)[] ancestors, CodeFileLocalArtifactTag[] ancestorTags,
        EditableCodeFile codeFile, SourceDocumentParserArgs args)
    {
        var testText = GetTestText(call);
        if (testText == null)
            return null;
        var testBody = GetTestBody(call);
        if (testBody == null)
            return null;

        var testName = ParseText(testText, out var testTags, out var testTextSpan, out var originalTestTitle, codeFile, args);
        var combinedTags = ancestorTags.Concat(testTags).GroupBy(t => t.Name).Select(g => g.First()).ToArray();
        var testCaseLink = args.TagServices.GetTestCaseLinkFromTags(combinedTags);

        List<LocalTestCaseDataRow>? dataRows = null;
        string[]? parameterNames = null;
        if (isParametrized)
        {
            var argNames = testBody.LambdaArgNames ?? [];
            if (argNames.Any())
            {
                parameterNames = argNames.ToArray();
            }
            var eachArray = call.TargetArguments.FirstOrDefault()?.FirstOrDefault()?.Array;
            if (argNames.Any() && eachArray != null && eachArray.Any())
            {
                dataRows = new();
                foreach (var arrayItem in eachArray)
                {
                    TypeScriptFunctionCallArgument.ArrayArgument paramValues =
                        arrayItem is TypeScriptFunctionCallArgument.LiteralArrayElement literal
                            ? [literal]
                            : (TypeScriptFunctionCallArgument.ArrayArgument)arrayItem;

                    dataRows.Add(new LocalTestCaseDataRow(
                        argNames.Select((name, index) => new KeyValuePair<string, string>(name, paramValues.ElementAtOrDefault(index)?.ToString() ?? ""))
                        ));
                }
            }
        }

        return new JestTestLocalTestCase(
            testName, originalTestTitle, ancestors.Select(a => a.OriginalTitle).ToArray(),
            combinedTags, testCaseLink, 
            call, testTextSpan,
            testedRule: ancestors.Any() ? new AcceptanceCriterion(ancestors.Last().Title, "") : null,
            dataRows: dataRows?.ToArray(), parameterNames: parameterNames);
    }

    private string ParseText(TypeScriptFunctionCallArgument textArg, out CodeFileLocalArtifactTag[] tags, out CodeSpan argValueSpan, out string originalTitle, EditableCodeFile codeFile, SourceDocumentParserArgs args)
    {
        var argValue = textArg.StringLiteral ?? textArg.Text;
        var argValueShift = textArg.Text.IndexOf(argValue);
        argValueSpan = codeFile.GetInnerSpan(textArg.Span, argValueShift, argValue.Length);
        originalTitle = argValue;
        var argValueSpanLocal = argValueSpan;
        return ParseTaggedText<CodeFileLocalArtifactTag>(argValue, 
            (tagValue, tagIndex) => 
                CreateCodeFileLocalTestCaseTag(
                    tagValue.Substring(1),
                    codeFile.GetInnerSpan(argValueSpanLocal, tagIndex, tagValue.Length),
                    args), 
            out tags);
    }

    private TypeScriptFunctionCallArgument? GetTestBody(TypeScriptFunctionCallBlock testCall)
    {
        var lambdaArg = testCall.CallArguments.ElementAtOrDefault(1);
        return lambdaArg is not { IsLambda: true } ? null : lambdaArg;
    }

    private TypeScriptFunctionCallArgument? GetTestText(TypeScriptFunctionCallBlock testCall)
    {
        return testCall.CallArguments.ElementAtOrDefault(0);
    }

    private TypeScriptFunctionCallBlock[]? GetDescribeBody(TypeScriptFunctionCallBlock describeCall)
    {
        var lambdaArg = describeCall.CallArguments.ElementAtOrDefault(1);
        return lambdaArg is not { IsLambda: true } ? null : lambdaArg.NestedCallBlocks?.ToArray();
    }

    private TypeScriptFunctionCallArgument? GetDescribeTitle(TypeScriptFunctionCallBlock describeCall)
    {
        return describeCall.CallArguments.ElementAtOrDefault(0);
    }

    private bool IsTest(TypeScriptFunctionCallBlock testBlock, out bool isParametrized)
    {
        isParametrized = false;
        if (testBlock is { IsSimpleCall: true, FunctionName: "test" or "it" }) 
            return true;
        if (testBlock.IsSimpleCall) 
            return false;
        if (TestExtensions.Any(ext => testBlock.FunctionName.StartsWith($"test.{ext}")) || 
            TestExtensions.Any(ext => testBlock.FunctionName.StartsWith($"it.{ext}"))) 
            return true;
        if (ParametrizedTestExtensions.Any(ext => testBlock.FunctionName.StartsWith($"test.{ext}")) ||
            ParametrizedTestExtensions.Any(ext => testBlock.FunctionName.StartsWith($"it.{ext}")))
        {
            isParametrized = true;
            return true;
        }

        return false;
    }

    private bool IsDescribe(TypeScriptFunctionCallBlock describeBlock)
    {
        return describeBlock is { IsSimpleCall: true, FunctionName: "describe" } ||
               (!describeBlock.IsSimpleCall && DescribeExtensions.Any(ext => describeBlock.FunctionName.StartsWith($"describe.{ext}")));
    }

    protected override CodeFileSourceDocumentUpdater CreateUpdater(EditableCodeFile codeFile, SourceDocumentParserArgs args)
    {
        return new JestTestUpdater(codeFile, args.Configuration, args.Tracer);
    }

    internal static string ParseTaggedText<TTag>(string taggedText, Func<string, int, TTag> tagFactory, out TTag[] tags)
    {
        var tagsBlockMatch = TagsRe.Match(taggedText);
        if (tagsBlockMatch.Success)
        {
            var tagListGroup = tagsBlockMatch.Groups["tagList"];
            tags = Regex.Matches(tagListGroup.Value, @"@[^\s]+").OfType<Match>()
                .Select(tagMatch => tagFactory(tagMatch.Value, tagListGroup.Index + tagMatch.Index))
                .ToArray();
            return taggedText.Remove(tagsBlockMatch.Index, tagsBlockMatch.Length).Trim();
        }

        tags = [];
        return taggedText;
    }

}