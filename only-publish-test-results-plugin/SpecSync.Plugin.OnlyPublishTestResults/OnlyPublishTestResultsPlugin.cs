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

        var parameters = args.GetParametersAs<OnlyPublishTestResultsPluginParameters>();
        parameters.Verify();

        args.ServiceRegistry.ProjectLoaderProvider
            .Register(new TestResultProjectLoader(parameters), ServicePriority.High);
        args.ServiceRegistry.SourceDocumentParserProvider
            .Register(new TestCaseResultSourceParser(), ServicePriority.High);
        args.ServiceRegistry.LocalArtifactAnalyzerProvider
            .Register(new TestCaseResultAnalyzer(), ServicePriority.High);
    }
}