using System;
using SpecSync.Parsing;
using SpecSync.Projects;

namespace ExcelTestSource.SpecSyncPlugin;

public class ExcelTestCaseContainer : ILocalTestCaseContainer
{
    public IBddProject BddProject { get; }
    public ISourceFile SourceFile { get; }
    public string Name { get; }
    public string Description => null;
    public ILocalTestCase[] LocalTestCases { get; }
    public ILocalTestCaseContainerUpdater Updater => null;
    public IKeywordParser KeywordParser { get; } = new NoKeywordParser();

    public ExcelTestCaseContainer(string name, IBddProject bddProject, ISourceFile sourceFile, ILocalTestCase[] localTestCases)
    {
        BddProject = bddProject;
        SourceFile = sourceFile;
        Name = name;
        LocalTestCases = localTestCases;
    }

    public string GetLocalTestCaseContainerSource() => null;

    public string GetLocalTestCaseSource(ILocalTestCase localTestCase) => null;
}