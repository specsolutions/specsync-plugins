using SpecSync.Plugin.JestTestSource;
using SpecSync.Plugin.JestTestSource.Jest;
using SpecSync.Plugin.JestTestSource.Projects;
using SpecSync.Plugins;

[assembly: SpecSyncPlugin(typeof(JestTestSourcePlugin))]

namespace SpecSync.Plugin.JestTestSource;

public class JestTestSourcePlugin : ISpecSyncPlugin
{
    public string Name => "Jest Test";

    public void Initialize(PluginInitializeArgs args)
    {
        args.Tracer.LogVerbose($"Initializing '{Name}' plugin...");

        args.ServiceRegistry.ProjectLoaderProvider
            .Register(new TypeScriptFolderProjectLoader(), ServicePriority.High);
        args.ServiceRegistry.SourceDocumentParserProvider
            .Register(new JestTestClassParser());
        args.ServiceRegistry.LocalArtifactAnalyzerProvider
            .Register(new JestTestAnalyzer());

        args.ServiceRegistry.TestResultLoaderProvider
            .Register(new JestJsonResultLoader());
        args.ServiceRegistry.TestResultMatcherProvider
            .Register(new JestTestResultMatcher(), ServicePriority.High);
    }
}