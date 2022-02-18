using System;
using MsTestTestSource.SpecSyncPlugin;
using SpecSync.AzureDevOps.Plugins;

[assembly: SpecSyncPlugin(typeof(MsTestTestSourcePlugin))]

namespace MsTestTestSource.SpecSyncPlugin
{
    public class MsTestTestSourcePlugin : ISpecSyncPlugin
    {
        public string Name => "Custom Test Result Support";

        public void Initialize(PluginInitializeArgs args)
        {
            args.Tracer.LogVerbose($"Initializing '{Name}' plugin...");
            args.ServiceRegistry.BddProjectLoaderProvider
                .Register(new MsTestTestSourceLoader());
            args.ServiceRegistry.LocalTestCaseContainerParserProvider
                .Register(new MsTestClassParser());
            args.ServiceRegistry.LocalTestCaseAnalyzerProvider
                .Register(new MsTestTestAnalyzer());

            args.ServiceRegistry.TestResultMatcherProvider
                .Register(new MsTestUnitTestMatcher(), ServicePriority.High);
        }
    }
}
