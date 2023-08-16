using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SpecSync.Parsing;
using SpecSync.PluginDependency.CSharpSource.TestMethodSource;
using SpecSync.PublishTestResults;
using SpecSync.PublishTestResults.Matchers;

namespace SpecSync.Plugin.TestNGTestSource.Java;

public class JavaTestResultMatcher : ITestRunnerResultMatcher
{
    public virtual string ServiceDescription => "Java Test Result";

    public virtual bool CanProcess(TestRunnerResultMatcherArgs args)
        => args.TestFrameworkIdentifier.IndexOf("java", StringComparison.InvariantCultureIgnoreCase) >= 0;

    public virtual MatchResultSelector GetLocalTestCaseResultSelector(ILocalTestCase localTestCase,
        ILocalTestCaseContainer localTestCaseContainer, TestRunnerResultMatcherArgs args)
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
                td.ClassName.Equals(fullClassName) && methodNameRe.IsMatch(td.Name ?? ""));
    }

    public virtual IDictionary<string, string> GetDataRow(TestRunTestResult testResult, TestRunTestDefinition testDefinition,
        ILocalTestCase localTestCase,
        ILocalTestCaseContainer localTestCaseContainer, TestRunnerResultMatcherArgs args)
    {
        return null;
    }
}