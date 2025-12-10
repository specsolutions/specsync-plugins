using SpecSync.Parsing;
using SpecSync.Plugin.TestNGTestSource.Java;
using SpecSync.Plugin.TestNGTestSource.JavaCode;

namespace SpecSync.Plugin.TestNGTestSource.TestNG;

public class TestNGTestMethodLocalTestCase(
    JavaMethodBlock methodBlock,
    JavaAnnotation? methodTestAnnotation,
    string name,
    IEnumerable<ILocalArtifactTag> tags,
    IdLink? testCaseLink,
    LocalTestCaseDataRow[]? dataRows = null,
    string? description = null,
    AcceptanceCriterion? testedRule = null)
    : JavaTestMethodLocalTestCase(methodBlock, name, tags, testCaseLink, dataRows, description, testedRule)
{
    public JavaAnnotation? MethodTestAnnotation { get; } = methodTestAnnotation;
}