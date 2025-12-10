using System.Reflection;
using System.Text.RegularExpressions;
using SpecSync.Configuration;
using SpecSync.Projects;
using SpecSync.PublishTestResults;
using SpecSync.Tracing;
using SpecSync.Utils;

namespace SpecSync.Plugin.OnlyPublishTestResults;

public class TestResultProjectLoader(OnlyPublishTestResultsPluginParameters parameters) : ISyncProjectLoader
{
    public string ServiceDescription => "Test Case Result loader";
    public string GetProjectDescription(SyncProjectLoaderArgs args) => "test results";

    public bool CanProcess(SyncProjectLoaderArgs args) 
        => args.LocalConfiguration.IsType(LocalProjectType.AutoDetect) ||
           args.LocalConfiguration.IsType("TestResult");

    public ISyncProject LoadProject(SyncProjectLoaderArgs args)
    {
        if (args.Command != "publish-test-results")
            throw new SpecSyncException($"Invalid SpecSync command '{args.Command}'. The 'OnlyPublishTestResultsPlugin' can only be used with the 'publish-test-results' command.");

        var commandContext = args.CommandContext;

        IEnumerable<LocalTestResult> GetFlattenLeafResults(LocalTestResult tr)
            => tr.InnerResults.Any() ? GetFlattenLeafInnerResults(tr) : [tr];

        IEnumerable<LocalTestResult> GetFlattenLeafInnerResults(LocalTestResult tr)
        {
            return tr.InnerResults.SelectMany(ir =>
                ir.InnerResults.Any() ?
                    GetFlattenLeafInnerResults(ir) : [ir]);
        }

        var testResults = commandContext.PublishTestResultContext.LocalTestRun.TestResults
            .SelectMany(GetFlattenLeafResults)
            .Select(tr => new { Result = tr, TestCaseId = GetTestCaseIdValue(tr) })
            .ToArray();

        var testResultsById = testResults
            .Where(r => r.TestCaseId != null)
            .GroupBy(r => r.TestCaseId!)
            .ToArray();

        if (testResultsById.Length == 0 && testResults.Length > 0)
        {
            args.Tracer.TraceWarning(new TraceWarningItem("The test results did not contain Test Case references. The OnlyPublishTestResults plugin can work only with test results that contain Test Case ID as a test result property."));
        }

        var documents = testResultsById
            .Select(rg => new TestCaseResultDocumentSource(rg.Key))
            .ToList();
        return new TestResultProject(args.BaseFolder, documents);
    }

    private string? GetTestCaseIdValue(LocalTestResult testResult)
    {
        var propertyValue = GetPropertyValue(testResult, parameters.TestCaseIdPropertyName);
        if (!string.IsNullOrEmpty(propertyValue) && !string.IsNullOrEmpty(parameters.ValueRegex))
        {
            var match = Regex.Match(propertyValue, parameters.ValueRegex);
            if (match.Success && match.Groups["id"].Success)
                propertyValue = match.Groups["id"].Value;
            else
                propertyValue = null;
        }
        return propertyValue;
    }

    private string? GetPropertyValue(LocalTestResult testResult, string propertyName)
    {
        return testResult.GetProperty<object>(propertyName)?.ToString() ??
               testResult.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)?.GetValue(testResult)?.ToString();
    }
}