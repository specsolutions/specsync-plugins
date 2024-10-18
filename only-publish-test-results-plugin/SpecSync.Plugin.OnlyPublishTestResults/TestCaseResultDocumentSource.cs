using SpecSync.Projects;

namespace SpecSync.Plugin.OnlyPublishTestResults;

public class TestCaseResultDocumentSource : ISourceFile
{
    public string Type => "Test Case Result";
    public string ProjectRelativePath { get; }

    public string TestCaseId { get; }

    public TestCaseResultDocumentSource(string testCaseId)
    {
        TestCaseId = testCaseId;
        ProjectRelativePath = $"Test Results of #{testCaseId}";
    }
}