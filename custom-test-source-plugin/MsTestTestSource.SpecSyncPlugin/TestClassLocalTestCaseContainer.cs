using System;
using System.Linq;
using SpecSync.AzureDevOps.Gherkin;
using SpecSync.AzureDevOps.Parsing;
using SpecSync.AzureDevOps.Projects;
using SpecSync.AzureDevOps.TfsIntegration;

namespace MsTestTestSource.SpecSyncPlugin
{
    public class TestClassLocalTestCaseContainer : ILocalTestCaseContainer
    {
        private readonly TestClassSource _testClassSource;
        public string Name => _testClassSource.TestClassType.FullName;
        public string Description => null;

        public IBddProject BddProject { get; }
        public ISourceFile SourceFile => _testClassSource;
        public ILocalTestCase[] LocalTestCases { get; }
        public ILocalTestCaseContainerUpdater Updater => null;
        public IKeywordParser KeywordParser => new NoKeywordParser(); // use GherkinKeywordParser.Default for G/W/T keywords

        public TestClassLocalTestCaseContainer(TestClassSource testClassSource, IBddProject bddProject, TestMethodLocalTestCase[] localTestCases)
        {
            _testClassSource = testClassSource;
            BddProject = bddProject;
            LocalTestCases = localTestCases.Cast<ILocalTestCase>().ToArray();
        }

        public string GetLocalTestCaseContainerSource()
        {
            return null; // the entire class source could be provided here (optional)
        }

        public string GetLocalTestCaseSource(ILocalTestCase localTestCase)
        {
            return (localTestCase as TestMethodLocalTestCase)?.SourceCode;
        }
    }
}
