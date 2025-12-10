using System.Text.RegularExpressions;
using SpecSync.Parsing;
using SpecSync.PublishTestResults;
using SpecSync.PublishTestResults.Matchers;
using SpecSync.TestMethodSource;

namespace SpecSync.Plugin.TestNGTestSource.Java;

public class JavaTestResultMatcher : ITestRunnerResultMatcher
{
    public virtual string ServiceDescription => "Java Test Result";

    public virtual bool CanProcess(TestRunnerResultMatcherArgs args)
        => args.TestFrameworkIdentifier.IndexOf("java", StringComparison.InvariantCultureIgnoreCase) >= 0;

    public MatchResultSelector GetLocalTestCaseResultSelector(ILocalTestCase localTestCase, ISourceDocument sourceDocument,
        TestRunnerResultMatcherArgs args)
    {
        var testMethodLocalTestCase = (TestMethodLocalTestCase)localTestCase;
        var methodName = testMethodLocalTestCase.MethodName;
        var className = testMethodLocalTestCase.ClassName;
        var fullClassName = testMethodLocalTestCase.Namespace == null
            ? className
            : $"{testMethodLocalTestCase.Namespace}.{className}";

        var methodNameRe = new Regex($@"^{Regex.Escape(methodName)}(\[.+\])?(\(.+\))?$");

        return new MatchResultSelector($"<className> is '{fullClassName}' && <name> matches '{methodNameRe}'",
            td =>
                fullClassName.Equals(td.ClassName) && methodNameRe.IsMatch(td.Name ?? ""));
    }

    public IDictionary<string, string>? GetInvocationArguments(LocalTestResult testResult, ILocalTestCase localTestCase,
        ISourceDocument sourceDocument, TestRunnerResultMatcherArgs args)
    {
        return null;
    }
}