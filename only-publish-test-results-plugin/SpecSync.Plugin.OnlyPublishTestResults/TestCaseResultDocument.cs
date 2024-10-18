using System;
using System.Linq;
using SpecSync.Parsing;
using SpecSync.Projects;

namespace SpecSync.Plugin.OnlyPublishTestResults;

public class TestCaseResultDocument : ILocalTestCaseContainer, ILocalTestCase
{
    private readonly TestCaseResultDocumentSource _source;

    #region ILocalTestCaseContainer
    public IBddProject BddProject { get; }
    public ISourceFile SourceFile => _source;
    public string Name => $"#{_source.TestCaseId}";
    public string Description => null;
    public ILocalTestCase[] LocalTestCases => new ILocalTestCase[] { this };
    public ILocalTestCaseContainerUpdater Updater => null;
    public IKeywordParser KeywordParser { get; } = new NoKeywordParser();

    public string GetLocalTestCaseContainerSource() => null;
    public string GetLocalTestCaseSource(ILocalTestCase localTestCase) => null;
    #endregion

    #region ILocalTestCase

    public string TestedRule => null;
    public ILocalTestCaseTag[] Tags => Array.Empty<ILocalTestCaseTag>();
    public TestCaseLink TestCaseLink { get; }
    public bool IsDataDrivenTest => false;
    public LocalTestCaseDataRow[] DataRows => null;
    public int TestCount => 1;

    #endregion

    public TestCaseResultDocument(IBddProject bddProject, TestCaseResultDocumentSource source, TestCaseLink testCaseLink)
    {
        _source = source;
        BddProject = bddProject;
        TestCaseLink = testCaseLink;
    }
}