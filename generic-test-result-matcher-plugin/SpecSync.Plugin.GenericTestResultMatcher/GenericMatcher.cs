using System.Text.RegularExpressions;
using SpecSync.Configuration;
using SpecSync.Parsing;
using SpecSync.Projects;
using SpecSync.PublishTestResults;
using SpecSync.PublishTestResults.Matchers;

namespace SpecSync.Plugin.GenericTestResultMatcher;

public class GenericMatcher(PluginParameters pluginParameters) : ITestRunnerResultMatcher
{
    public virtual string ServiceDescription => "Generic Result Matcher";

    public virtual bool CanProcess(TestRunnerResultMatcherArgs args) => true;

    public MatchResultSelector GetLocalTestCaseResultSelector(ILocalTestCase localTestCase, ISourceDocument sourceDocument, TestRunnerResultMatcherArgs args)
    {
        var selectors = new List<MatchResultSelector?>
        {
            CreateNameMatch(localTestCase, sourceDocument, args),
            CreateClassNameMatch(localTestCase, sourceDocument, args),
            CreateMethodNameMatch(localTestCase, sourceDocument, args),
            CreateStdOutMatch(localTestCase, sourceDocument, args)
        };
        foreach (var testResultProperty in pluginParameters.TestResultProperties)
        {
            selectors.Add(CreateTestResultPropertyMatch(localTestCase, sourceDocument, args, testResultProperty));
        }

        return CombineSelectors(selectors.ToArray());
    }

    private MatchResultSelector? CreateTestResultPropertyMatch(ILocalTestCase localTestCase, ISourceDocument sourceDocument, TestRunnerResultMatcherArgs args, KeyValuePair<string, string> testResultProperty)
    {
        return CreateMatch($"TestResultProperty:{testResultProperty.Key}", 
            testResultProperty.Value, 
            tr => tr.GetProperty<object>(testResultProperty.Key)?.ToString(), 
            localTestCase, sourceDocument, args);
    }

    private MatchResultSelector? CreateNameMatch(ILocalTestCase localTestCase, ISourceDocument sourceDocument, TestRunnerResultMatcherArgs args)
    {
        return CreateMatch("name", pluginParameters.Name, tr => tr.GetName(), localTestCase, sourceDocument, args);
    }

    private MatchResultSelector? CreateClassNameMatch(ILocalTestCase localTestCase, ISourceDocument sourceDocument, TestRunnerResultMatcherArgs args)
    {
        return CreateMatch("className", pluginParameters.ClassName, tr => tr.ClassName, localTestCase, sourceDocument, args);
    }

    private MatchResultSelector? CreateMethodNameMatch(ILocalTestCase localTestCase, ISourceDocument sourceDocument, TestRunnerResultMatcherArgs args)
    {
        return CreateMatch("methodName", pluginParameters.MethodName, tr => tr.MethodName, localTestCase, sourceDocument, args);
    }

    private MatchResultSelector? CreateStdOutMatch(ILocalTestCase localTestCase, ISourceDocument sourceDocument, TestRunnerResultMatcherArgs args)
    {
        return CreateMatch("stdOut", pluginParameters.StdOut, tr => tr.StdOut, localTestCase, sourceDocument, args);
    }

    // ReSharper disable once UnusedParameter.Local
    private MatchResultSelector? CreateMatch(string paramName, string? paramRe, Func<LocalTestResult, string?> paramSelector, ILocalTestCase localTestCase, ISourceDocument sourceDocument, TestRunnerResultMatcherArgs args)
    {
        if (string.IsNullOrEmpty(paramRe))
            return null;

        var regexString = paramRe!;
        regexString = regexString.Replace("{local-test-case-name}", Regex.Escape(localTestCase.Name));
        regexString = regexString.Replace("{local-test-case-container-name}", Regex.Escape(sourceDocument.Name));
        regexString = regexString.Replace("{local-test-case-container-filename}", Regex.Escape(GetFileName(sourceDocument)));
        regexString = regexString.Replace("{test-case-id}", Regex.Escape(localTestCase.IdLink!.Id.GetExistingIdAsString()));
        var regex = new Regex(regexString);

        return new MatchResultSelector($"<{paramName}> matches /{regexString}/",
            tr => regex.IsMatch(paramSelector(tr) ?? ""));
    }

    private string GetFileName(ISourceDocument sourceDocument)
    {
        try
        {
            return sourceDocument.SourceReference.GetFileName();
        }
        catch
        {
            return "Unknown";
        }
    }

    public IDictionary<string, string>? GetInvocationArguments(LocalTestResult testResult, ILocalTestCase localTestCase,
        ISourceDocument sourceDocument, TestRunnerResultMatcherArgs args)
    {
        return null;
    }

    private MatchResultSelector CombineSelectors(params MatchResultSelector?[] selectors)
    {
        var validSelectors = selectors.Where(s => s != null).Cast<MatchResultSelector>().ToArray();
        if (validSelectors.Length == 0)
            throw new SpecSyncConfigurationException("At least one of the plugin parameters 'name', 'className', 'methodName' or 'stdOut' has to be specified.");
        return new MatchResultSelector(
            string.Join(" and ", validSelectors.Select(s => s.DiagMessage)),
            tr => validSelectors.All(s => s.Func(tr))
        );
    }
}