using System;
using System.Collections.Generic;
using SpecSync.Configuration;
using SpecSync.Plugin.PostmanTestSource;
using SpecSync.Plugins;

[assembly: SpecSyncPlugin(typeof(PostmanTestSourcePlugin))]

namespace SpecSync.Plugin.PostmanTestSource;

public class PostmanTestSourcePlugin : ISpecSyncPlugin
{
    public class Parameters
    {
        public static readonly Dictionary<string, object> Defaults = new()
        {
            { nameof(PostmanApiKey), "{env:POSTMAN_API_KEY}" },
            { nameof(MetadataHeading), "SpecSync" },
        };
        public string PostmanApiKey { get; set; }
        public string CollectionId { get; set; }
        public string MetadataHeading { get; set; }

        public void CheckParameters(string pluginName)
        {
            if (string.IsNullOrWhiteSpace(CollectionId))
                throw new SpecSyncConfigurationException($"The 'collectionId' parameter must be provided for the {pluginName} plugin.");
            if (string.IsNullOrWhiteSpace(PostmanApiKey))
                throw new SpecSyncConfigurationException($"The 'postmanApiKey' parameter must be provided or the 'POSTMAN_API_KEY' environment variable must be set for the {pluginName} plugin.");
            if (string.IsNullOrWhiteSpace(MetadataHeading))
                throw new SpecSyncConfigurationException($"The 'metadataHeading' parameter cannot be empty for the {pluginName} plugin.");
        }
    }

    public string Name => "Postman Test Case Source";

    public void Initialize(PluginInitializeArgs args)
    {
        args.Tracer.LogVerbose($"Initializing '{Name}' plugin...");

        var parameters = args.GetParametersAs<Parameters>(Parameters.Defaults);
        parameters.CheckParameters(Name);

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