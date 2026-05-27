using System.Diagnostics;
using SpecSync.IO;
using SpecSync.Parsing;
using SpecSync.Plugin.JestTestSource.TypeScriptCode;
using SpecSync.TestMethodSource;
using SpecSync.Tracing;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.JestTestSource.TypeScript;

public abstract class TypeScriptTestFunctionParserBase : ISourceDocumentParser
{
    public abstract string ServiceDescription { get; }

    public bool CanProcess(SourceDocumentParserArgs args)
        => args.SourceReference.ProjectRelativePath.EndsWith(".ts", StringComparison.OrdinalIgnoreCase) ||
           args.SourceReference.ProjectRelativePath.EndsWith(".tsx", StringComparison.OrdinalIgnoreCase);

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

    protected abstract CodeFileSourceDocumentUpdater CreateUpdater(EditableCodeFile codeFile, SourceDocumentParserArgs args);

    protected virtual EditableCodeFile LoadCodeFile(string filePath, IFileSystem fileSystem)
    {
        return EditableCodeFile.Load(filePath, fileSystem);
    }

    protected internal abstract IEnumerable<TestMethodLocalTestCase> GetTestMethodLocalTestCases(SourceDocumentParserArgs args, EditableCodeFile codeFile);

    protected virtual TypeScriptFunctionCallBlock[] ParseCallBlocks(EditableCodeFile codeFile, SourceDocumentParserArgs args)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        try
        {
            var parser = new TypeScriptFunctionCallBlockParser();
            return parser.Parse(codeFile, (_, innerParser, output) =>
            {
                if (!string.IsNullOrWhiteSpace(output))
                    args.Tracer.LogVerbose($"Parser output: {output.Trim()}");
                if (innerParser.NumberOfSyntaxErrors > 0)
                {
                    args.Tracer.TraceWarning(new TraceWarningItem(
                        $"Invalid TypeScript syntax.{Environment.NewLine}Parser errors:{Environment.NewLine}{output.TrimEnd()}"));
                }
            });
        }
        finally
        {
            stopwatch.Stop();
            args.Tracer.LogDiag("Jest", $"TypeScript file parse time: {stopwatch.ElapsedMilliseconds:D}");
        }
    }

    protected virtual CodeFileLocalArtifactTag CreateCodeFileLocalTestCaseTag(string tagName, CodeSpan sourceSpan, SourceDocumentParserArgs args)
    {
        args.TagServices.TryParseTagName(tagName, out var prefix, out var artifactId, out var artifactLabel);
        return new CodeFileLocalArtifactTag(tagName, sourceSpan, prefix, artifactId, artifactLabel);
    }
}