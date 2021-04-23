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
            args.Tracer.LogVerbose("Initializing MsTest test source plugin...");
            args.ServiceRegistry.BddProjectLoaderProvider
                .Register(new MsTestTestSourceLoader());
            args.ServiceRegistry.LocalTestCaseContainerParserProvider
                .Register(new MsTestClassParser());
            args.ServiceRegistry.LocalTestCaseAnalyzerProvider
                .Register(new MsTestTestAnalyzer());

            args.ServiceRegistry.TestResultMatcher
                .Register(new MsTestUnitTestMatcher(), ServicePriority.High);
        }
    }
}
