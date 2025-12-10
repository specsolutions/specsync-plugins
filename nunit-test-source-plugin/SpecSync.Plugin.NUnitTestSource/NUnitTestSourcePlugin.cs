using SpecSync.Plugin.NUnitTestSource;
using SpecSync.PluginDependency.CSharpSource.NUnit;
using SpecSync.PluginDependency.CSharpSource.Projects;
using SpecSync.Plugins;

[assembly: SpecSyncPlugin(typeof(NUnitTestSourcePlugin))]

namespace SpecSync.Plugin.NUnitTestSource;

public class NUnitTestSourcePlugin : ISpecSyncPlugin
{
    public string Name => "NUnit C# Test";

    public void Initialize(PluginInitializeArgs args)
    {
        args.Tracer.LogVerbose($"Initializing '{Name}' plugin...");
        args.ServiceRegistry.ProjectLoaderProvider
            .Register(new CSharpProjectLoader(), ServicePriority.High);
        args.ServiceRegistry.SourceDocumentParserProvider
            .Register(new NUnitTestClassParser());
        args.ServiceRegistry.LocalArtifactAnalyzerProvider
            .Register(new NUnitTestAnalyzer());

        args.ServiceRegistry.TestResultLoaderProvider
            .Register(new NUnit2XmlResultLoader());
        args.ServiceRegistry.TestResultLoaderProvider
            .Register(new NUnit3XmlResultLoader());
        args.ServiceRegistry.TestResultLoaderProvider
            .Register(new NUnitXmlResultLoader());
        args.ServiceRegistry.TestResultMatcherProvider
            .Register(new NUnitTestResultMatcher(), ServicePriority.High);
    }
}