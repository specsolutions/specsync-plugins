using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
                TestSteps = GetSteps(testMethodTestCase).ToList()
            };
        }

        public IEnumerable<TestStepSourceData> GetSteps(TestMethodLocalTestCase testMethodTestCase)
        {
            var stepComments = Regex.Matches(testMethodTestCase.SourceCode,
                    @"//\s*(?<key>Step|Assertion):\s*(?<stepText>.*)")
                .OfType<Match>()
                .Select(m => 
                    new
                    {
                        Key = m.Groups["key"].Value.Trim(),
                        Value = m.Groups["stepText"].Value.Trim()
                    });

            return stepComments
                .Select(c => new TestStepSourceData
                {
                    Text = new ParameterizedText(c.Value),
                    IsThenStep = c.Key == "Assertion",
                    Keyword = c.Key + " "
                });
        }
    }
}