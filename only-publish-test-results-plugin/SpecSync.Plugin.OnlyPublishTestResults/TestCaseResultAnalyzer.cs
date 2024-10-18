using System;
using System.Collections.Generic;
using SpecSync.Analyzing;

namespace SpecSync.Plugin.OnlyPublishTestResults;

public class TestCaseResultAnalyzer : ILocalTestCaseAnalyzer
{
    public string ServiceDescription => "Test Case Result analyzer";

    public bool CanProcess(LocalTestCaseAnalyzerArgs args)
        => args.LocalTestCase is TestCaseResultDocument;

    public TestCaseSourceData Analyze(LocalTestCaseAnalyzerArgs args)
    {
        var excelLocalTestCase = (TestCaseResultDocument)args.LocalTestCase;
        return new TestCaseSourceData
        {
            Title = excelLocalTestCase.Name,
            Links = args.TagServices.GetLinkData(excelLocalTestCase),
            Tags = args.TagServices.GetTagData(args.TestCaseSyncContext),
            TestSteps = new List<TestStepSourceData>(),
            ParamValues = Array.Empty<TestCaseParameters>()
        };
    }
}