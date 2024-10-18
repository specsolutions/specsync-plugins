using System.Collections.Generic;
using System.Globalization;
using SpecSync.Projects;

namespace SpecSync.Plugin.OnlyPublishTestResults;

public class TestResultProject : IBddProject
{
    private readonly List<TestCaseResultDocumentSource> _documents;

    public string Type => "TestResult";
    public CultureInfo DefaultCulture => null;
    public IEnumerable<ISourceFile> LocalTestContainerFiles => _documents;
    public string ProjectFolder { get; }

    public TestResultProject(string projectFolder, List<TestCaseResultDocumentSource> documents)
    {
        _documents = documents;
        ProjectFolder = projectFolder;
    }

    public string GetFullPath(string projectRelativePath)
    {
        return projectRelativePath;
    }
}