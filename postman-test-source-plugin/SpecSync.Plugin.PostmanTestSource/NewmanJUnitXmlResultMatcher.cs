using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SpecSync.Parsing;
using SpecSync.PublishTestResults;
using SpecSync.PublishTestResults.Matchers;

namespace SpecSync.Plugin.PostmanTestSource;

public class NewmanJUnitXmlResultMatcher : ITestRunnerResultMatcher
{
    public const string TestFrameworkIdentifier = "executor://postman-newman/junit-xml";

    public string ServiceDescription => "Postman Newman";

    public bool CanProcess(TestRunnerResultMatcherArgs args) 
        => args.TestFrameworkIdentifier.Equals(TestFrameworkIdentifier, StringComparison.InvariantCultureIgnoreCase);

    public MatchResultSelector GetLocalTestCaseResultSelector(ILocalTestCase localTestCase,
        ILocalTestCaseContainer localTestCaseContainer, TestRunnerResultMatcherArgs args)
    {
        var localTestCaseName = Regex.Escape(localTestCase.Name);
        var regexString = @$"^(.*\/\s*)?{localTestCaseName}$";
        var regex = new Regex(regexString);
        return new MatchResultSelector($"<name> matches /{regexString}/",
            td => regex.IsMatch(td.Name ?? ""));
    }

    public IDictionary<string, string> GetDataRow(TestRunTestResult testResult, TestRunTestDefinition testDefinition,
        ILocalTestCase localTestCase,
        ILocalTestCaseContainer localTestCaseContainer, TestRunnerResultMatcherArgs args)
        => null;
}