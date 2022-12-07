using System;
using ScenarioOutlinePerExamplesTestCase.SpecSyncPlugin;
using SpecSync.Analyzing;
using SpecSync.Plugins;

[assembly: SpecSyncPlugin(typeof(ScenarioOutlinePerExamplesTestCasePlugin))]

namespace ScenarioOutlinePerExamplesTestCase.SpecSyncPlugin
{
    public class ScenarioOutlinePerExamplesTestCasePlugin : ISpecSyncPlugin
    {
        public string Name => "Scenario Outline per Examples Test Case";

        public void Initialize(PluginInitializeArgs args)
        {
            args.Tracer.LogVerbose($"Initializing '{Name}' plugin...");
            args.ServiceRegistry.LocalTestCaseContainerParserProvider
                .Register(new ScenarioOutlinePerExamplesParser(args.ServiceRegistry.GetBuiltInService<ITagServices>()), ServicePriority.High);
        }
    }
}
