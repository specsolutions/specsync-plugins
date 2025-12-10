using SpecSync.Parsing;
using SpecSync.Plugin.TestNGTestSource.Java;
using SpecSync.Plugin.TestNGTestSource.JavaCode;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.TestNGTestSource.TestNG;

public class TestNGTestClassParser : JavaTestClassParserBase
{
    public const string TestNGPackage = "org.testng.annotations";
    public const string TestAttributeName = "Test";
    public const string GroupElementName = "groups";
    public static readonly string[] NonTestAttributeNames =
    [
        "BeforeSuite",
        "AfterSuite",
        "BeforeTest",
        "AfterTest",
        "BeforeGroups",
        "AfterGroups",
        "BeforeClass",
        "AfterClass",
        "BeforeMethod",
        "AfterMethod",
        "DataProvider",
        "Factory"
    ];

    public override string ServiceDescription => "TestNG test class parser";

    public override string GetTestCaseLinkTemplate(ILocalArtifact localTestCase)
    {
        throw new NotSupportedException(); // link template is implemented in TestNGTestUpdater
    }

    public override IEnumerable<ILocalArtifactTag> FindTags(JavaMethodBlock testJavaMethodBlock, SourceDocumentParserArgs args)
    {
        var categoryAttributes = testJavaMethodBlock.ClassAnnotations
            .Concat(testJavaMethodBlock.Annotations)
            .Where(a => IsAttributeOf(a, TestNGPackage, TestAttributeName) &&
                        a.Elements.Any(e => e.Name == GroupElementName))
            .SelectMany(a => a.Elements.First(e => e.Name == GroupElementName).GetElementArrayValue() ?? [], 
                (a,g) => 
                    (
                        Annotation: a, 
                        Group: g,
                        GroupValue: g.GetStringValue()!
                    ))
            .GroupBy(g => g.GroupValue)
            .Select(g => g.First());
        foreach (var group in categoryAttributes)
        {
            yield return CreateCodeFileLocalTestCaseTag(group.GroupValue, group.Group.ValueSpan, args);
        }
    }

    public override bool IsTestMethodBlock(JavaMethodBlock methodBlock)
    {
        return
            HasAttributeOf(methodBlock.Annotations, TestNGPackage, TestAttributeName) ||
            (HasAttributeOf(methodBlock.ClassAnnotations, TestNGPackage, TestAttributeName) && 
             !HasAttributeOfAny(methodBlock.Annotations, TestNGPackage, NonTestAttributeNames));
    }

    protected override JavaTestMethodLocalTestCase CreateTestMethodLocalTestCase(JavaMethodBlock testJavaMethodBlock, ILocalArtifactTag[] tags, IdLink? testCaseLink, LocalTestCaseDataRow[]? dataRows, SourceDocumentParserArgs args)
    {
        var methodTestAnnotation = testJavaMethodBlock.Annotations
            .FirstOrDefault(a => IsAttributeOf(a, TestNGPackage, TestAttributeName));

        return new TestNGTestMethodLocalTestCase(testJavaMethodBlock, methodTestAnnotation, GetTestName(testJavaMethodBlock), tags, testCaseLink, dataRows, GetDescription(testJavaMethodBlock));
    }

    protected override JavaTestUpdater CreateUpdater(EditableCodeFile codeFile, SourceDocumentParserArgs args)
    {
        return new TestNGTestUpdater(codeFile, args.Configuration, args.Tracer);
    }
}