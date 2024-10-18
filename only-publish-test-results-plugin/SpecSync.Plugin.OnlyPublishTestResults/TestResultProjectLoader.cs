using System.Collections.Generic;
using System.Linq;
using SpecSync.Configuration;
using SpecSync.Projects;
using SpecSync.PublishTestResults;
using SpecSync.Tracing;
using SpecSync.Utils;

namespace SpecSync.Plugin.OnlyPublishTestResults;

public class TestResultProjectLoader : IBddProjectLoader
{
    private readonly OnlyPublishTestResultsPluginParameters _parameters;

    public TestResultProjectLoader(OnlyPublishTestResultsPluginParameters parameters)
    {
        _parameters = parameters;
    }

    public string ServiceDescription => "Test Case Result loader";
    public string GetSourceDescription(BddProjectLoaderArgs args) => "test results";

    public bool CanProcess(BddProjectLoaderArgs args) 
        => args.LocalConfiguration.IsType(LocalProjectType.AutoDetect) ||
           args.LocalConfiguration.IsType("TestResult");

    public IBddProject LoadProject(BddProjectLoaderArgs args)
    {
        if (args.Command != "publish-test-results")
            throw new SpecSyncException($"Invalid SpecSync command '{args.Command}'. The 'OnlyPublishTestResultsPlugin' can only be used with the 'publish-test-results' command.");

        var synchronizationContext = args.GetSynchronizationContext();

        IEnumerable<TestRunTestResult> GetFlattenLeafResults(TestRunTestResult tr)
            => tr.InnerResults.Any() ? GetFlattenLeafInnerResults(tr) : new[] { tr };

        IEnumerable<TestRunTestResult> GetFlattenLeafInnerResults(TestRunTestResult tr)
        {
            return tr.InnerResults.SelectMany(ir =>
                ir.InnerResults.Any() ?
                    GetFlattenLeafInnerResults(ir) :
                    new[] { ir });
        }

        var testResults = synchronizationContext.PublishTestResultContext.LocalTestRun.TestDefinitions
            .SelectMany(td => td.Results.SelectMany(GetFlattenLeafResults)
                .Select(tr => new { TestRunTestDefinition = td, Result = tr, TestCaseId = tr.GetProperty<object>(_parameters.TestCaseIdPropertyName)?.ToString() }))
            .ToArray();

        var testResultsById = testResults
            .Where(r => r.TestCaseId != null)
            .GroupBy(r => r.TestCaseId)
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
}