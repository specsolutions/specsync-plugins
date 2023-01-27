using System;
using SpecSync.Plugin.NUnitTestSource;
using SpecSync.PluginDependency.CSharpSource.NUnit;
using SpecSync.PluginDependency.CSharpSource.Projects;
using SpecSync.Plugins;

[assembly: SpecSyncPlugin(typeof(NUnitTestSourcePlugin))]

namespace SpecSync.Plugin.NUnitTestSource
{
    public class NUnitTestSourcePlugin : ISpecSyncPlugin
    {
        public string Name => "NUnit C# Test";

        public void Initialize(PluginInitializeArgs args)
        {
            args.Tracer.LogVerbose($"Initializing '{Name}' plugin...");
            args.ServiceRegistry.BddProjectLoaderProvider
                .Register(new CSharpProjectLoader(), ServicePriority.High);
            args.ServiceRegistry.LocalTestCaseContainerParserProvider
                .Register(new NUnitTestClassParser());
            args.ServiceRegistry.LocalTestCaseAnalyzerProvider
            .Register(new NUnitTestAnalyzer());

            args.ServiceRegistry.TestResultMatcherProvider
                .Register(new NUnitTestResultMatcher(), ServicePriority.High);
        }
    }
}
