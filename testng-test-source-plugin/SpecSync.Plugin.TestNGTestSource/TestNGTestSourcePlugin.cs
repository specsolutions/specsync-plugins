using System;
using SpecSync.Plugin.TestNGTestSource;
using SpecSync.Plugin.TestNGTestSource.Java;
using SpecSync.Plugin.TestNGTestSource.Projects;
using SpecSync.Plugin.TestNGTestSource.TestNG;
using SpecSync.Plugins;

[assembly: SpecSyncPlugin(typeof(TestNGTestSourcePlugin))]

namespace SpecSync.Plugin.TestNGTestSource
{
    public class TestNGTestSourcePlugin : ISpecSyncPlugin
    {
        public string Name => "TestNG Java Test";

        public void Initialize(PluginInitializeArgs args)
        {
            args.Tracer.LogVerbose($"Initializing '{Name}' plugin...");

            args.ServiceRegistry.BddProjectLoaderProvider
                .Register(new JavaFolderProjectLoader(), ServicePriority.High);
            args.ServiceRegistry.LocalTestCaseContainerParserProvider
                .Register(new TestNGTestClassParser());
            args.ServiceRegistry.LocalTestCaseAnalyzerProvider
                .Register(new TestNGTestAnalyzer());

            args.ServiceRegistry.TestResultLoaderProvider
                .Register(new JUnitXmlResultLoader());
            args.ServiceRegistry.TestResultLoaderProvider
                .Register(new SurefireXmlResultLoader());
            args.ServiceRegistry.TestResultMatcherProvider
                .Register(new JavaTestResultMatcher(), ServicePriority.High);
        }
    }
}
