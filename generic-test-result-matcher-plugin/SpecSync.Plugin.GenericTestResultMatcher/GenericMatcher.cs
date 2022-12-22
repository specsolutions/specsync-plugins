using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SpecSync.Configuration;
using SpecSync.Parsing;
using SpecSync.PublishTestResults;
using SpecSync.PublishTestResults.Matchers;

namespace SpecSync.Plugin.GenericTestResultMatcher;

public class GenericMatcher : ITestRunnerResultMatcher
{
    public virtual string ServiceDescription => "Generic Result Matcher";

    public virtual bool CanProcess(TestRunnerResultMatcherArgs args) => true;

    private readonly PluginParameters _pluginParameters;

    public GenericMatcher(PluginParameters pluginParameters)
    {
        _pluginParameters = pluginParameters;
    }

    public MatchResultSelector GetLocalTestCaseResultSelector(ILocalTestCase localTestCase, ILocalTestCaseContainer localTestCaseContainer, TestRunnerResultMatcherArgs args)
    {
        return CombineSelectors(
            CreateNameMatch(localTestCase, localTestCaseContainer, args),
            CreateClassNameMatch(localTestCase, localTestCaseContainer, args),
            CreateMethodNameMatch(localTestCase, localTestCaseContainer, args)
        );
    }

    private MatchResultSelector CreateNameMatch(ILocalTestCase localTestCase, ILocalTestCaseContainer localTestCaseContainer, TestRunnerResultMatcherArgs args)
    {
        return CreateMatch("name", _pluginParameters.Name, td => td.Name, localTestCase, localTestCaseContainer, args);
    }

    private MatchResultSelector CreateClassNameMatch(ILocalTestCase localTestCase, ILocalTestCaseContainer localTestCaseContainer, TestRunnerResultMatcherArgs args)
    {
        return CreateMatch("className", _pluginParameters.ClassName, td => td.ClassName, localTestCase, localTestCaseContainer, args);
    }

    private MatchResultSelector CreateMethodNameMatch(ILocalTestCase localTestCase, ILocalTestCaseContainer localTestCaseContainer, TestRunnerResultMatcherArgs args)
    {
        return CreateMatch("methodName", _pluginParameters.MethodName, td => td.MethodName, localTestCase, localTestCaseContainer, args);
    }

    private MatchResultSelector CreateMatch(string paramName, string paramRe, Func<TestRunTestDefinition, string> paramSelector, ILocalTestCase localTestCase, ILocalTestCaseContainer localTestCaseContainer, TestRunnerResultMatcherArgs args)
    {
        if (string.IsNullOrEmpty(paramRe))
            return null;

        var regexString = paramRe;
        regexString = regexString.Replace("{local-test-case-name}", Regex.Escape(localTestCase.Name));
        regexString = regexString.Replace("{local-test-case-container-name}", Regex.Escape(localTestCaseContainer.Name));
        regexString = regexString.Replace("{local-test-case-container-filename}", Regex.Escape(GetFileName(localTestCaseContainer)));
        var regex = new Regex(regexString);

        return new MatchResultSelector($"<{paramName}> matches /{regexString}/",
            td => regex.IsMatch(paramSelector(td) ?? ""));
    }

    private string GetFileName(ILocalTestCaseContainer localTestCaseContainer)
    {
        try
        {
            var relativePath = localTestCaseContainer.SourceFile?.ProjectRelativePath ?? "Unknown";
            return Path.GetFileName(relativePath);
        }
        catch
        {
            return "Unknown";
        }
    }

    public IDictionary<string, string> GetDataRow(TestRunTestResult testResult, TestRunTestDefinition testDefinition, ILocalTestCase localTestCase,
        ILocalTestCaseContainer localTestCaseContainer, TestRunnerResultMatcherArgs args)
    {
        return null;
    }

    private MatchResultSelector CombineSelectors(params MatchResultSelector[] selectors)
    {
        var validSelectors = selectors.Where(s => s != null).ToArray();
        if (validSelectors.Length == 0)
            throw new SpecSyncConfigurationException("At least one of the plugin parameters 'name', 'className' or 'methodName' has to be specified.");
        return new MatchResultSelector(
            string.Join(" and ", validSelectors.Select(s => s.DiagMessage)),
            td => validSelectors.All(s => s.Func(td))
        );
    }
}