using SpecSync.Plugin.ScenarioOutlineAsNormalTestCase;
using SpecSync.Plugins;

[assembly: SpecSyncPlugin(typeof(ScenarioOutlineAsNormalTestCaseFormatPlugin))]

namespace SpecSync.Plugin.ScenarioOutlineAsNormalTestCase;

public class ScenarioOutlineAsNormalTestCaseFormatPlugin : ISpecSyncPlugin
{
    public string Name => "Format scenario outline as normal Test Case";

    public void Initialize(PluginInitializeArgs args)
    {
        args.Tracer.LogVerbose($"Initializing '{Name}' plugin...");
        args.ServiceRegistry.LocalArtifactAnalyzerProvider
            .Register(new ScenarioOutlineAsNormalTestCaseGherkinAnalyzer(), ServicePriority.High);
    }
}