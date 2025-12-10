using SpecSync.Plugin.TestNGTestSource;
using SpecSync.Plugin.TestNGTestSource.Java;
using SpecSync.Plugin.TestNGTestSource.Projects;
using SpecSync.Plugin.TestNGTestSource.TestNG;
using SpecSync.Plugins;

[assembly: SpecSyncPlugin(typeof(TestNGTestSourcePlugin))]

namespace SpecSync.Plugin.TestNGTestSource;

public class TestNGTestSourcePlugin : ISpecSyncPlugin
{
    public string Name => "TestNG Java Test";

    public void Initialize(PluginInitializeArgs args)
    {
        args.Tracer.LogVerbose($"Initializing '{Name}' plugin...");

        args.ServiceRegistry.ProjectLoaderProvider
            .Register(new JavaFolderProjectLoader(), ServicePriority.High);
        args.ServiceRegistry.SourceDocumentParserProvider
            .Register(new TestNGTestClassParser());
        args.ServiceRegistry.LocalArtifactAnalyzerProvider
            .Register(new TestNGTestAnalyzer());

        args.ServiceRegistry.TestResultLoaderProvider
            .Register(new TestNGJUnitXmlResultLoader());
        args.ServiceRegistry.TestResultLoaderProvider
            .Register(new SurefireXmlResultLoader());
        args.ServiceRegistry.TestResultMatcherProvider
            .Register(new JavaTestResultMatcher(), ServicePriority.High);
    }
}