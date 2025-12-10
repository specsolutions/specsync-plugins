using SpecSync.Parsing;
using SpecSync.Utils.Code;
using System.Text.RegularExpressions;
using SpecSync.IO;
using SpecSync.Plugin.TestNGTestSource.JavaCode;
using SpecSync.TestMethodSource;

namespace SpecSync.Plugin.TestNGTestSource.Java;

public abstract class JavaTestClassParserBase : ISourceDocumentParser
{
    public abstract string ServiceDescription { get; }

    public bool CanProcess(SourceDocumentParserArgs args)
        => args.SourceReference.ProjectRelativePath.EndsWith(".java", StringComparison.OrdinalIgnoreCase);

    public virtual ISourceDocument Parse(SourceDocumentParserArgs args)
    {
        var filePath = args.Project.GetFullPath(args.SourceReference);
        var codeFile = LoadCodeFile(filePath, args.CommandContext.FileSystem);

        var testMethodTestCases = GetTestMethodLocalTestCases(args, codeFile).ToArray();

        var updater = CreateUpdater(codeFile, args);
        var keywordParser = CreateKeywordParser(args);
        return new TestClassSourceDocument(codeFile, args.SourceReference, args.SourceReference.ProjectRelativePath, args.Project, testMethodTestCases, updater, keywordParser);
    }

    protected virtual IKeywordParser CreateKeywordParser(SourceDocumentParserArgs args) => NoKeywordParser.Instance;

    protected virtual JavaTestUpdater CreateUpdater(EditableCodeFile codeFile, SourceDocumentParserArgs args)
    {
        return new JavaTestUpdater(codeFile, GetTestCaseLinkTemplate, args.Configuration, args.Tracer);
    }

    public abstract string GetTestCaseLinkTemplate(ILocalArtifact localTestCase);

    protected virtual EditableCodeFile LoadCodeFile(string filePath, IFileSystem fileSystem)
    {
        return EditableCodeFile.Load(filePath, fileSystem);
    }

    protected internal virtual IEnumerable<TestMethodLocalTestCase> GetTestMethodLocalTestCases(
        SourceDocumentParserArgs args, EditableCodeFile codeFile)
    {
        var methodBlocks = ParseMethodBlocks(codeFile);
        var testJavaMethodBlocks = GetTestJavaMethodBlocks(methodBlocks, args);

        foreach (var testJavaMethodBlock in testJavaMethodBlocks)
        {
            var tags = FindTags(testJavaMethodBlock, args).ToArray();
            var testCaseLink = args.TagServices.GetTestCaseLinkFromTags(tags);

            yield return CreateTestMethodLocalTestCase(testJavaMethodBlock, tags, testCaseLink, GetDataRows(testJavaMethodBlock), args);
        }
    }

    public virtual LocalTestCaseDataRow[]? GetDataRows(JavaMethodBlock javaMethodBlock)
    {
        return null;
    }

    protected virtual JavaTestMethodLocalTestCase CreateTestMethodLocalTestCase(JavaMethodBlock testJavaMethodBlock, ILocalArtifactTag[] tags, IdLink? testCaseLink, LocalTestCaseDataRow[]? dataRows, SourceDocumentParserArgs args)
    {
        return new JavaTestMethodLocalTestCase(testJavaMethodBlock, GetTestName(testJavaMethodBlock), tags, testCaseLink, dataRows, GetDescription(testJavaMethodBlock));
    }

    protected virtual string? GetDescription(JavaMethodBlock testJavaMethodBlock)
    {
        var docComment = testJavaMethodBlock.DocCommentSpan?.Text;
        if (docComment == null)
            return null;

        var description = Regex.Replace(docComment, @"^\s*\/*\**\s?", "", RegexOptions.Multiline);
        description = Regex.Replace(description, @"^(\s*\r?\n)+", "");
        description = Regex.Replace(description, @"\r?\n\@.*", "", RegexOptions.Singleline);
        return description.TrimEnd();
    }

    protected virtual string GetTestName(JavaMethodBlock javaMethodBlock)
    {
        return $"{javaMethodBlock.ClassName}.{javaMethodBlock.MethodName}";
    }

    protected virtual JavaMethodBlock[] ParseMethodBlocks(EditableCodeFile codeFile)
    {
        var parser = new JavaMethodBlockParser();
        return parser.Parse(codeFile);
    }
    protected virtual CodeFileLocalArtifactTag CreateCodeFileLocalTestCaseTag(string tagName, CodeSpan sourceSpan, SourceDocumentParserArgs args)
    {
        args.TagServices.TryParseTagName(tagName, out var prefix, out var artifactId, out var artifactLabel);
        return new CodeFileLocalArtifactTag(tagName, sourceSpan, prefix, artifactId, artifactLabel);
    }

    public abstract IEnumerable<ILocalArtifactTag> FindTags(JavaMethodBlock testJavaMethodBlock, SourceDocumentParserArgs args);

    protected virtual IEnumerable<JavaMethodBlock> GetTestJavaMethodBlocks(JavaMethodBlock[] methodBlocks, SourceDocumentParserArgs args)
    {
        return methodBlocks.Where(IsTestMethodBlock);
    }

    public abstract bool IsTestMethodBlock(JavaMethodBlock methodBlock);

    protected bool IsAttributeOf(JavaAnnotation attribute, string expectedNamespace, string expectedName)
    {
        bool IsAttributeName(string name) => name == expectedName;

        return IsAttributeName(attribute.Name) ||
               (attribute.Name.StartsWith(expectedNamespace + ".") &&
                IsAttributeName(attribute.Name.Substring(expectedNamespace.Length + 1)));
    }

    protected bool IsAttributeOfAny(JavaAnnotation attribute, string expectedNamespace, string[] expectedNames)
    {
        bool IsAttributeName(string name) => expectedNames.Contains(name);

        return IsAttributeName(attribute.Name) ||
               (attribute.Name.StartsWith(expectedNamespace + ".") &&
                IsAttributeName(attribute.Name.Substring(expectedNamespace.Length + 1)));
    }

    protected bool HasAttributeOf(JavaAnnotation[] attributes, string expectedNamespace, string expectedName)
    {
        return attributes.Any(a => IsAttributeOf(a, expectedNamespace, expectedName));
    }

    protected bool HasAttributeOfAny(JavaAnnotation[] attributes, string expectedNamespace, string[] expectedNames)
    {
        return attributes.Any(a => IsAttributeOfAny(a, expectedNamespace, expectedNames));
    }

}