using System;
using SpecSync.Parsing;
using SpecSync.Projects;

namespace SpecSync.Plugin.ExcelTestSource;

public class ExcelTestCaseContainer : ILocalTestCaseContainer
{
    public IBddProject BddProject { get; }
    public ISourceFile SourceFile { get; }
    public string Name { get; }
    public string Description => null;
    public ILocalTestCase[] LocalTestCases { get; }
    public ILocalTestCaseContainerUpdater Updater { get; }
    public IKeywordParser KeywordParser { get; } = new NoKeywordParser();

    public ExcelTestCaseContainer(string name, IBddProject bddProject, ISourceFile sourceFile, ILocalTestCase[] localTestCases, ILocalTestCaseContainerUpdater updater)
    {
        BddProject = bddProject;
        SourceFile = sourceFile;
        Name = name;
        LocalTestCases = localTestCases;
        Updater = updater;
    }

    public string GetLocalTestCaseContainerSource() => null;

    public string GetLocalTestCaseSource(ILocalTestCase localTestCase) => null;
}