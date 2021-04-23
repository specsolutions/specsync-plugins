using System;
using System.Linq;
using System.Reflection;
using SpecSync.AzureDevOps.Parsing;

namespace MsTestTestSource.SpecSyncPlugin
{
    public class MsTestClassParser : ILocalTestCaseContainerParser
    {
        public const string TEST_CLASS_TYPE = "TestClass";
        public string ServiceDescription => "MsTest TestClass parser";
        private SourceCodeProvider _sourceCodeProvider;

        public bool CanProcess(LocalTestCaseContainerParseArgs args)
            => TEST_CLASS_TYPE.Equals(args.SourceFile.Type);

        public ILocalTestCaseContainer Parse(LocalTestCaseContainerParseArgs args)
        {
            var testClassSource = (TestClassSource)args.SourceFile;

            var testMethodInfos = testClassSource.TestClassType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(IsTestMethod)
                .ToArray();

            var testMethodTestCases = testMethodInfos
                .Select(info => CreateTestMethodLocalTestCase(info, args))
                .ToArray();

            return new TestClassLocalTestCaseContainer(testClassSource, args.BddProject, testMethodTestCases);
        }

        private bool IsTestMethod(MethodInfo methodInfo)
        {
            const string TEST_METHOD_ATTRIBUTE = "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute";
            return methodInfo.GetCustomAttributesData().Any(a => TEST_METHOD_ATTRIBUTE.Equals(a.AttributeType.FullName));
        }

        private TestMethodLocalTestCase CreateTestMethodLocalTestCase(MethodInfo mi, LocalTestCaseContainerParseArgs args)
        {
            var categories = GetTestCategories(mi);
            var tags = categories.Select(c => new LocalTestCaseTag(c)).ToArray();
            var testCaseLink = args.TagServices.GetTestCaseLinkFromTags(tags);

            EnsureSourceCodeProvider(args);
            var sourceCode = _sourceCodeProvider.GetMethodSource(mi);

            return new TestMethodLocalTestCase(mi, tags, testCaseLink, sourceCode);
        }

        private string[] GetTestCategories(MethodInfo methodInfo)
        {
            const string TEST_CATEGORY_ATTRIBUTE = "Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute";
            return methodInfo.GetCustomAttributesData().Where(a => TEST_CATEGORY_ATTRIBUTE.Equals(a.AttributeType.FullName))
                .Select(a => a.ConstructorArguments.Select(ca => ca.Value.ToString()).FirstOrDefault())
                .Where(c => c != null)
                .ToArray();
        }

        private void EnsureSourceCodeProvider(LocalTestCaseContainerParseArgs args)
        {
            if (_sourceCodeProvider != null)
                return;
            _sourceCodeProvider = new SourceCodeProvider(((MsTestTestProject)args.BddProject).AssemblyPath, args.Tracer);
        }
    }
}