using System;
using System.Linq;
using SpecSync.Analyzing;

namespace SpecSync.Plugin.ExcelTestSource;

public class ExcelTestCaseAnalyzer : ILocalTestCaseAnalyzer
{
    public string ServiceDescription => "Excel Test Case analyzer";

    public bool CanProcess(LocalTestCaseAnalyzerArgs args)
        => args.LocalTestCase is ExcelLocalTestCase;

    public TestCaseSourceData Analyze(LocalTestCaseAnalyzerArgs args)
    {
        var excelLocalTestCase = (ExcelLocalTestCase)args.LocalTestCase;
        return new TestCaseSourceData
        {
            Title = excelLocalTestCase.Name,
            Links = args.TagServices.GetLinkData(excelLocalTestCase),
            Tags = args.TagServices.GetTagData(args.TestCaseSyncContext),
            TestSteps = excelLocalTestCase.Steps.ToList(),
            ParamValues = Array.Empty<TestCaseParameters>()
        };
    }
}