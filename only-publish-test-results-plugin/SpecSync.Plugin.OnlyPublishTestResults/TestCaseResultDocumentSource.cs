using SpecSync.Projects;

namespace SpecSync.Plugin.OnlyPublishTestResults;

public class TestCaseResultDocumentSource(string testCaseId) : ISourceReference
{
    public string Type => "Test Case Result";
    public string ProjectRelativePath { get; } = $"Test Results of #{testCaseId}";

    public string TestCaseId { get; } = testCaseId;
}