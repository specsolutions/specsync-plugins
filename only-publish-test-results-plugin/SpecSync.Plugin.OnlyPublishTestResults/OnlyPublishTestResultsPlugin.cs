using System;
using System.Linq;
using SpecSync.Configuration;
using SpecSync.Plugin.OnlyPublishTestResults;
using SpecSync.Plugins;

[assembly: SpecSyncPlugin(typeof(OnlyPublishTestResultsPlugin))]

namespace SpecSync.Plugin.OnlyPublishTestResults;

public class OnlyPublishTestResultsPlugin : ISpecSyncPlugin
{
    public string Name => "Only Publish Test Results";

    public void Initialize(PluginInitializeArgs args)
    {
        args.Tracer.LogVerbose($"Initializing '{Name}' plugin...");

        var parameters = OnlyPublishTestResultsPluginParameters.FromPluginParameters(args.Parameters);

        args.ServiceRegistry.BddProjectLoaderProvider
            .Register(new TestResultProjectLoader(parameters), ServicePriority.High);
        args.ServiceRegistry.LocalTestCaseContainerParserProvider
            .Register(new TestCaseResultSourceParser(), ServicePriority.High);
        args.ServiceRegistry.LocalTestCaseAnalyzerProvider
            .Register(new TestCaseResultAnalyzer(), ServicePriority.High);
    }
}