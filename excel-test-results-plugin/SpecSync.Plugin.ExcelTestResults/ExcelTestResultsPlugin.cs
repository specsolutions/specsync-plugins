using System;
using SpecSync.Plugin.ExcelTestResults;
using SpecSync.Plugins;

[assembly: SpecSyncPlugin(typeof(ExcelTestResultsPlugin))]

namespace SpecSync.Plugin.ExcelTestResults;

public class ExcelTestResultsPlugin : ISpecSyncPlugin
{
    public string Name => "Excel Test Result";

    public void Initialize(PluginInitializeArgs args)
    {
        args.Tracer.LogVerbose($"Initializing '{Name}' plugin...");

        var parameters = ExcelResultParameters.FromPluginParameters(args.Parameters);

        args.ServiceRegistry.TestResultLoaderProvider.Register(new ExcelTestResultLoader(parameters), ServicePriority.High);
        args.ServiceRegistry.TestResultMatcherProvider.Register(new ExcelTestResultMatcher(parameters), ServicePriority.High);
    }
}