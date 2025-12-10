using SpecSync.Parsing;
using SpecSync.Plugin.TestNGTestSource.JavaCode;
using SpecSync.TestMethodSource;

namespace SpecSync.Plugin.TestNGTestSource.Java;

public class JavaTestMethodLocalTestCase(
    JavaMethodBlock methodBlock,
    string name,
    IEnumerable<ILocalArtifactTag> tags,
    IdLink? testCaseLink,
    LocalTestCaseDataRow[]? dataRows = null,
    string? description = null,
    AcceptanceCriterion? testedRule = null)
    : TestMethodLocalTestCase(name, methodBlock.PackageName, methodBlock.ClassName, methodBlock.MethodName, tags,
        testCaseLink, methodBlock.SourceSpan, dataRows, description, testedRule, JavaTabSize)
{
    public const int JavaTabSize = 4;
    public JavaMethodBlock MethodBlock { get; } = methodBlock;
}