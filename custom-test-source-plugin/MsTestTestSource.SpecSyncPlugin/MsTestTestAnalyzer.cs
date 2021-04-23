using System;
using SpecSync.AzureDevOps.Analyzing;
using SpecSync.AzureDevOps.TfsIntegration.Diff;

namespace MsTestTestSource.SpecSyncPlugin
{
    public class MsTestTestAnalyzer : ILocalTestCaseAnalyzer
    {
        public string ServiceDescription => "MsTest test analyzer";

        public bool CanProcess(LocalTestCaseAnalyzerArgs args)
            => args.LocalTestCase is TestMethodLocalTestCase;

        public TestCaseSourceData Analyze(LocalTestCaseAnalyzerArgs args)
        {
            var testMethodTestCase = (TestMethodLocalTestCase) args.LocalTestCase;
            return new TestCaseSourceData
            {
                Title = testMethodTestCase.Name,
                Links = args.TagServices.GetLinkData(testMethodTestCase),
                Tags = args.TagServices.GetTagData(testMethodTestCase),
            };
        }
    }
}