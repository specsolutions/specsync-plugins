using System.Globalization;
using SpecSync.Projects;

namespace SpecSync.Plugin.OnlyPublishTestResults;

public class TestResultProject(string projectFolder, List<TestCaseResultDocumentSource> documents)
    : ISyncProject
{
    public string Type => "TestResult";
    public CultureInfo? DefaultCulture => null;
    public IEnumerable<ISourceReference> SourceReferences => documents;
    public string ProjectFolder { get; } = projectFolder;

    public string GetFullPath(ISourceReference sourceReference)
    {
        return sourceReference.ProjectRelativePath;
    }
}