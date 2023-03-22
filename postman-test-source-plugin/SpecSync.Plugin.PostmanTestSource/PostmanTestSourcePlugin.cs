using System;
using SpecSync.Configuration;
using SpecSync.Plugin.PostmanTestSource;
using SpecSync.Plugins;

[assembly: SpecSyncPlugin(typeof(PostmanTestSourcePlugin))]

namespace SpecSync.Plugin.PostmanTestSource;

public class PostmanTestSourcePlugin : ISpecSyncPlugin
{
    public class Parameters
    {
        public string CollectionId { get; set; }
    }

    public string Name => "Postman Test Case Source";

    public void Initialize(PluginInitializeArgs args)
    {
        args.Tracer.LogVerbose($"Initializing '{Name}' plugin...");

        var parameters = args.GetParametersAs<Parameters>();
        if (string.IsNullOrWhiteSpace(parameters.CollectionId))
            throw new SpecSyncConfigurationException($"The 'collectionId' parameter must be provided for the {Name} plugin.");

        args.ServiceRegistry.BddProjectLoaderProvider
            .Register(new PostmanCollectionLoader(parameters));
        args.ServiceRegistry.LocalTestCaseContainerParserProvider
            .Register(new PostmanFolderItemParser());
        args.ServiceRegistry.LocalTestCaseAnalyzerProvider
            .Register(new PostmanTestItemAnalyzer());
        args.ServiceRegistry.TestResultLoaderProvider
            .Register(new NewmanJUnitXmlResultLoader());
        args.ServiceRegistry.TestResultMatcherProvider
            .Register(new NewmanJUnitXmlResultMatcher());
    }
}