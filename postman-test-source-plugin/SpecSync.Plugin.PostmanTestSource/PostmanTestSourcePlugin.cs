using System;
using SpecSync.Plugin.PostmanTestSource;
using SpecSync.Plugins;

[assembly: SpecSyncPlugin(typeof(PostmanTestSourcePlugin))]

namespace SpecSync.Plugin.PostmanTestSource;

public class PostmanTestSourcePlugin : ISpecSyncPlugin
{
    public string Name => "Postman Test Case Source";

    public void Initialize(PluginInitializeArgs args)
    {
        args.Tracer.LogVerbose($"Initializing '{Name}' plugin...");

        args.ServiceRegistry.BddProjectLoaderProvider
            .Register(new PostmanCollectionLoader());
        args.ServiceRegistry.LocalTestCaseContainerParserProvider
            .Register(new PostmanCollectionParser());
        args.ServiceRegistry.LocalTestCaseAnalyzerProvider
            .Register(new PostmanTestItemAnalyzer());
    }
}