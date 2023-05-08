using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SpecSync.Analyzing;
using SpecSync.Plugin.PostmanTestSource.Projects;

namespace SpecSync.Plugin.PostmanTestSource;

public class PostmanTestItemAnalyzer : ILocalTestCaseAnalyzer
{
    public string ServiceDescription => "Postman Test Analyzer";

    public bool CanProcess(LocalTestCaseAnalyzerArgs args)
        => args.LocalTestCase is PostmanTestItem;

    public TestCaseSourceData Analyze(LocalTestCaseAnalyzerArgs args)
    {
        var testItem = (PostmanTestItem)args.LocalTestCase;

        return new TestCaseSourceData
        {
            Title = testItem.Name,
            Links = args.TagServices.GetLinkData(testItem),
            Tags = args.TagServices.GetTagData(args.TestCaseSyncContext),
            TestSteps = GetSteps(testItem).ToList(),
            AttachedFiles = args.TagServices.GetAttachedFileData(testItem, args),
        };
    }

    private IEnumerable<TestStepSourceData> GetSteps(PostmanTestItem testItem)
    {
        foreach (var requestItem in testItem.GetRequestItems())
        {
            if (requestItem.Request != null)
            {
                yield return new TestStepSourceData
                {
                    Keyword = (requestItem.Request.Method ?? "GET") + " ",
                    Text = new ParameterizedText(requestItem.Request.Url?.Raw ?? "???")
                };
            }

            if (requestItem.Events != null)
            {
                foreach (var testEvent in requestItem.Events.Where(e => "test".Equals(e.Listen) && e.Script?.Exec != null))
                {
                    foreach (var execLine in testEvent.Script.Exec)
                    {
                        var match = Regex.Match(execLine, @"^(?<before>.*)\bpm\.test\((?<expected>.+),");
                        if (match.Success && !match.Groups["before"].Value.Contains("//"))
                            yield return new TestStepSourceData
                            {
                                IsThenStep = true,
                                Keyword = "pm.test ",
                                Text = new ParameterizedText(SimplifyJsString(match.Groups["expected"].Value))
                            };
                    }
                }
            }
        }
    }

    private string SimplifyJsString(string value)
    {
        value = value.Trim();
        var separators = new[] { "\"", "'", "`" };
        foreach (var separator in separators)
        {
            if (value.Length >= separator.Length * 2 && 
                value.StartsWith(separator) && value.EndsWith(separator) && 
                !value.Substring(separator.Length, value.Length - separator.Length * 2).Contains(separator))
            {
                return value.Substring(separator.Length, value.Length - separator.Length * 2);
            }
        }
        return value;
    }
}