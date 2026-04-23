using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SpecSync.PublishTestResults;
using SpecSync.PublishTestResults.Loaders;

namespace SpecSync.Plugin.JestTestSource.Jest;

public class JestJsonResultLoader : ITestResultLoader
{
    private static readonly Regex AnsiEscapeCodeRegex = new(@"\x1B\[[0-?]*[ -/]*[@-~]", RegexOptions.Compiled);

    public const string JestJsonResultFormat = "JestJson";
    public const string JestJsonFrameworkIdentifier = "executor://jest/json";
    public const string TestResultNameProperty = "test-result-name";

    public string ServiceDescription => $"{JestJsonResultFormat}: Jest JSON result";

    public bool CanProcess(TestResultLoaderProviderArgs args)
        => args.TestResultConfiguration.IsResultFormat(JestJsonResultFormat) &&
           ".json".Equals(Path.GetExtension(args.TestResultFilePath), StringComparison.InvariantCultureIgnoreCase);

    public LocalTestRun LoadTestResult(TestResultLoaderProviderArgs args)
    {
        var jsonContent = args.CommandContext.FileSystem.File.ReadAllText(args.TestResultFilePath);
        var jestResult = Deserialize(jsonContent);

        var localTestRun = new LocalTestRun
        {
            TestFrameworkIdentifier = JestJsonFrameworkIdentifier,
            StartTime = GetTimeFromEpoch(jestResult.StartTime),
            FinishTime = GetTimeFromEpoch(jestResult.TestResults?.Max(r => r.EndTime) ?? jestResult.StartTime)
        };

        foreach (var testResult in jestResult.TestResults ?? [])
        {
            foreach (var assertionResult in testResult.AssertionResults ?? [])
            {
                var errorMessage = assertionResult.FailureMessages?.Any() == true
                    ? string.Join(Environment.NewLine, assertionResult.FailureMessages.Select(RemoveAnsiEscapeCodes))
                    : null;

                var localTestResult = new LocalTestResult
                {
                    Name = RemoveTags(assertionResult.FullName) ?? RemoveTags(assertionResult.Title),
                    TestName = RemoveTags(assertionResult.Title) ?? RemoveTags(assertionResult.FullName) ?? string.Empty,
                    ClassName = GetClassName(assertionResult.AncestorTitles),
                    MethodName = assertionResult.Title,
                    Outcome = GetOutcome(assertionResult.Status),
                    Duration = GetDuration(assertionResult.Duration),
                    ErrorMessage = errorMessage
                };

                var tags = GetTags(assertionResult.Title);
                if (assertionResult.AncestorTitles != null)
                    foreach (var ancestorTitle in assertionResult.AncestorTitles)
                    {
                        tags = [..tags, ..GetTags(ancestorTitle)];
                    }
                localTestResult.Categories.AddRange(tags.Select(t => t.Substring(1)));

                localTestResult.AddProperty(TestResultNameProperty, testResult.Name);

                localTestRun.TestResults.Add(localTestResult);
            }
        }

        return localTestRun;
    }

    private static string RemoveAnsiEscapeCodes(string text)
    {
        return AnsiEscapeCodeRegex.Replace(text, string.Empty);
    }

    private string[] GetTags(string? taggedName)
    {
        if (taggedName == null)
            return [];

        JestTestClassParser.ParseTaggedText(taggedName, (tag, _) => tag, out string[] tags);
        return tags;
    }

    private DateTimeOffset? GetTimeFromEpoch(long? epochTime)
    {
        if (epochTime == null)
            return null;
        return DateTimeOffset.FromUnixTimeMilliseconds(epochTime.Value);
    }

    private string? RemoveTags(string? name)
    {
        if (name == null)
            return null;

        var tagBlockMatches = JestTestClassParser.TagsRe.Matches(name);
        foreach (var tagBlockMatch in tagBlockMatches.OfType<Match>().OrderByDescending(m => m.Index))
        {
            name = name.Remove(tagBlockMatch.Index, tagBlockMatch.Length);
            bool spaceRemovedAfter = false;
            while (name.Length > tagBlockMatch.Index && char.IsWhiteSpace(name[tagBlockMatch.Index]))
            {
                name = name.Remove(tagBlockMatch.Index, 1);
                spaceRemovedAfter = true;
            }

            if (!spaceRemovedAfter)
            {
                int index = tagBlockMatch.Index - 1;
                while (index >= 0 && char.IsWhiteSpace(name[index]))
                {
                    name = name.Remove(index, 1);
                    index--;
                }
            }
        }

        return name.Trim();
    }

    public static string GetClassName(string[]? ancestorTitles)
    {
        if (ancestorTitles is { Length: > 0 })
            return string.Join("/", ancestorTitles);

        return string.Empty;
    }

    private static TimeSpan? GetDuration(int? duration)
        => duration == null ? null : TimeSpan.FromMilliseconds(duration.Value);

    private static TestOutcome GetOutcome(string? status)
        => status?.ToLowerInvariant() switch
        {
            "passed" => TestOutcome.Passed,
            "failed" => TestOutcome.Failed,
            "skipped" => TestOutcome.NotExecuted,
            "pending" => TestOutcome.Pending,
            "todo" => TestOutcome.Pending,
            "disabled" => TestOutcome.NotExecuted,
            "focused" => TestOutcome.Passed, // some tests were skipped
            _ => TestOutcome.Unknown
        };

    public JestJsonRootResult Deserialize(string jsonString)
    {
        return JsonConvert.DeserializeObject<JestJsonRootResult>(jsonString) ?? new JestJsonRootResult();
    }

    public class JestJsonRootResult
    {
        public long? StartTime { get; set; }
        public List<JestJsonTestResult>? TestResults { get; set; }
    }

    public class JestJsonTestResult
    {
        public string? Name { get; set; }
        public long? StartTime { get; set; }
        public long? EndTime { get; set; }
        public string? Message { get; set; }
        public string? Summary { get; set; }
        public string? Status { get; set; }
        public List<JestJsonAssertionResult>? AssertionResults { get; set; }
    }

    public class JestJsonAssertionResult
    {
        public string[]? AncestorTitles { get; set; }
        public int? Duration { get; set; } // milliseconds
        public long? StartAt { get; set; } // not used
        public bool? Failing { get; set; } // not used
        public JestJsonFailureDetail[]? FailureDetails { get; set; }
        public string[]? FailureMessages { get; set; }
        public string[]? RetryReasons { get; set; }
        public string? FullName { get; set; }
        public string? Status { get; set; }
        public string? Title { get; set; }
        public JestJsonCallsite? Location { get; set; }
        public int? Invocations { get; set; }
        public int? NumPassingAsserts { get; set; }
    }

    public class JestJsonFailureDetail
    {
        public JestJsonMatcherResult? MatcherResult { get; set; }
    }

    public class JestJsonMatcherResult
    {
        public bool? Actual { get; set; }
        public bool? Expected { get; set; }
        public bool? Pass { get; set; }
        public string? Name { get; set; }
        public string? Message { get; set; }
        public string? Stack { get; set; }
    }
    public class JestJsonCallsite
    {
        public int? Column { get; set; }
        public int? Line { get; set; }
    }
}
