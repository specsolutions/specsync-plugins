using SpecSync.Plugin.MsTestTestSource;
using SpecSync.PluginDependency.CSharpSource.MsTest;
using SpecSync.PluginDependency.CSharpSource.Projects;
using SpecSync.Plugins;

[assembly: SpecSyncPlugin(typeof(MsTestTestSourcePlugin))]

namespace SpecSync.Plugin.MsTestTestSource;

public class MsTestTestSourcePlugin : ISpecSyncPlugin
{
    public string Name => "MsTest C# Test";

    public void Initialize(PluginInitializeArgs args)
    {
        args.Tracer.LogVerbose($"Initializing '{Name}' plugin...");
        args.ServiceRegistry.ProjectLoaderProvider
            .Register(new CSharpProjectLoader(), ServicePriority.High);
        args.ServiceRegistry.SourceDocumentParserProvider
            .Register(new MsTestTestClassParser());
        args.ServiceRegistry.LocalArtifactAnalyzerProvider
            .Register(new MsTestTestAnalyzer());

        args.ServiceRegistry.TestResultMatcherProvider
            .Register(new MsTestUnitTestMatcher(), ServicePriority.High);
    }
}