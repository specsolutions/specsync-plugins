using SpecSync.Plugin.GenericTestResultMatcher;
using SpecSync.Plugins;

[assembly: SpecSyncPlugin(typeof(GenericResultMatcherPlugin))]

namespace SpecSync.Plugin.GenericTestResultMatcher;

public class GenericResultMatcherPlugin : ISpecSyncPlugin
{
    public string Name => "Genetic result matcher";

    public void Initialize(PluginInitializeArgs args)
    {
        args.Tracer.LogVerbose($"Initializing '{Name}' plugin...");

        var parameters = PluginParameters.FromPluginParameters(args.Parameters);

        args.ServiceRegistry.TestResultMatcherProvider.Register(new GenericMatcher(parameters), ServicePriority.High);
    }
}