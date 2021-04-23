using System;
using SpecSync.AzureDevOps.Projects;

namespace MsTestTestSource.SpecSyncPlugin
{
    public class TestClassSource : ISourceFile
    {
        public string Type => MsTestClassParser.TEST_CLASS_TYPE;
        public string ProjectRelativePath => TestClassType.FullName;
        public Type TestClassType { get; }

        public TestClassSource(Type testClassType)
        {
            TestClassType = testClassType;
        }
    }
}
