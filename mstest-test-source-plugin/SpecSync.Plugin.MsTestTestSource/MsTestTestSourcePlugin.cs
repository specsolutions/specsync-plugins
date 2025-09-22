using System;
using SpecSync.Plugin.MsTestTestSource;
using SpecSync.PluginDependency.CSharpSource.MsTest;
using SpecSync.PluginDependency.CSharpSource.Projects;
using SpecSync.Plugins;

[assembly: SpecSyncPlugin(typeof(MsTestTestSourcePlugin))]

namespace SpecSync.Plugin.MsTestTestSource
{
    public class MsTestTestSourcePlugin : ISpecSyncPlugin
    {
        public string Name => "MsTest C# Test";

        public void Initialize(PluginInitializeArgs args)
        {
            args.Tracer.LogVerbose($"Initializing '{Name}' plugin...");
            args.ServiceRegistry.BddProjectLoaderProvider
                .Register(new CSharpProjectLoader(), ServicePriority.High);
            args.ServiceRegistry.LocalTestCaseContainerParserProvider
                .Register(new MsTestTestClassParser());
            args.ServiceRegistry.LocalTestCaseAnalyzerProvider
                .Register(new MsTestTestAnalyzer());

            args.ServiceRegistry.TestResultMatcherProvider
                .Register(new MsTestUnitTestMatcher(), ServicePriority.High);
        }
    }
}
