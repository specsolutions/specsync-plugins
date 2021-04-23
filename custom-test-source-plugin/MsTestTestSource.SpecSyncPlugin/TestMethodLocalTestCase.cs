using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gherkin.Ast;
using SpecSync.AzureDevOps.Parsing;

namespace MsTestTestSource.SpecSyncPlugin
{
    public class TestMethodLocalTestCase : ILocalTestCase
    {
        private readonly MethodInfo _testMethodInfo;
        public string SourceCode { get; }

        public string Name => _testMethodInfo.Name;
        public string Description => null; // provide description maybe from code comments
        public string TestedRule => null; // tested business rule description
        public ILocalTestCaseTag[] Tags { get; }
        public bool IsDataDrivenTest => false;
        public LocalTestCaseDataRow[] DataRows => null; // to be used for data-driven tests (optional)
        public int TestCount => 1;
        public TestCaseLink TestCaseLink { get; }

        public TestMethodLocalTestCase(MethodInfo testMethodInfo, IEnumerable<ILocalTestCaseTag> tags,
            TestCaseLink testCaseLink, string sourceCode)
        {
            Tags = tags.ToArray();
            TestCaseLink = testCaseLink;
            SourceCode = sourceCode;
            _testMethodInfo = testMethodInfo;
        }
    }
}