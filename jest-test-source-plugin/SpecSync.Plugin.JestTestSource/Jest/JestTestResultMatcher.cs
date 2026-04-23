using System.Text.RegularExpressions;
using SpecSync.Parsing;
using SpecSync.PublishTestResults;
using SpecSync.PublishTestResults.Matchers;
using SpecSync.Synchronization;

namespace SpecSync.Plugin.JestTestSource.Jest;

public class JestTestResultMatcher : ITestRunnerResultMatcher
{
    public virtual string ServiceDescription => "Jest Test Result";

    public virtual bool CanProcess(TestRunnerResultMatcherArgs args)
        => args.TestFrameworkIdentifier.Equals(JestJsonResultLoader.JestJsonFrameworkIdentifier, StringComparison.InvariantCultureIgnoreCase);

    public MatchResultSelector GetLocalTestCaseResultSelector(ILocalTestCase localTestCase, ISourceDocument sourceDocument, TestRunnerResultMatcherArgs args)
    {
        var jestTest = (JestTestLocalTestCase)localTestCase;
        var className = jestTest.ClassName;
        var methodName = jestTest.MethodName;
        var id = jestTest.IdLink?.Id;

        return new MatchResultSelector((id == null ? "" : $"(<categories> contains '{jestTest.IdLink?.LinkPrefix}{args.Configuration.Synchronization.TagPrefixSeparators.First()}{id}') or ") + $"(<className> is '{className}' && <methodName> is '{methodName}')",
            tr =>
                (id != null && id == GetId(tr, args)) ||
                (className.Equals(tr.ClassName) && methodName.Equals(tr.TestName)));
    }

    private ManagedWorkItemIdentifier? GetId(LocalTestResult testResult, TestRunnerResultMatcherArgs args)
    {
        var idLink = args.TagServices.GetTestCaseLinkFromTags(testResult.Categories.Select(c => new LocalArtifactTag(c)));
        return idLink?.Id;
    }

    public IDictionary<string, string>? GetInvocationArguments(LocalTestResult testResult, ILocalTestCase localTestCase, ISourceDocument sourceDocument, TestRunnerResultMatcherArgs args)
    {
        var jestTest = (JestTestLocalTestCase)localTestCase;
        if (testResult.MethodName == null || !jestTest.IsDataDrivenTest() || jestTest.MethodName.Equals(testResult.MethodName))
            return null;

        var regex = Regex.Escape(jestTest.MethodName);
        regex = Regex.Replace(regex, "%[psdifjo]", "(.*?)");
        regex = Regex.Replace(regex, "%[#$%]", "(?:.*?)");

        Regex argMatchRe;
        try
        {
            argMatchRe = new Regex(regex);
        }
        catch
        {
            return null;
        }

        var match = argMatchRe.Match(testResult.MethodName);
        if (!match.Success)
            return null;

        var groups = match.Groups.OfType<Group>().Skip(1).ToArray();
        return jestTest.ParameterNames!
            .Select((name, index) => (Name: name, Value: groups.ElementAtOrDefault(index)?.Value ?? ""))
            .ToDictionary(item => item.Name, item => item.Value);
    }
}