using System;
using ScenarioOutlineAsNormalTestCase.SpecSyncPlugin;
using SpecSync.AzureDevOps.Plugins;

[assembly: SpecSyncPlugin(typeof(ScenarioOutlineAsNormalTestCaseFormatPlugin))]

namespace ScenarioOutlineAsNormalTestCase.SpecSyncPlugin
{
    public class ScenarioOutlineAsNormalTestCaseFormatPlugin : ISpecSyncPlugin
    {
        public string Name => "Format scenario outline as normal Test Case";

        public void Initialize(PluginInitializeArgs args)
        {
            args.Tracer.LogVerbose($"Initializing '{Name}' plugin...");
            args.ServiceRegistry.LocalTestCaseAnalyzerProvider
                .Register(new ScenarioOutlineAsNormalTestCaseGherkinAnalyzer(), ServicePriority.High);
        }
    }
}
