using System;
using MyCustomTestResultMatch.SpecSyncPlugin;
using SpecSync.AzureDevOps.Plugins;

[assembly: SpecSyncPlugin(typeof(CustomTestResultMatchPlugin))]

namespace MyCustomTestResultMatch.SpecSyncPlugin
{
    public class CustomTestResultMatchPlugin : ISpecSyncPlugin
    {
        public string Name => "Custom Test Result Support";

        public void Initialize(PluginInitializeArgs args)
        {
            args.Tracer.LogVerbose($"Initializing {Name} plugin...");
            args.ServiceRegistry.TestResultMatcherProvider.Register(new CustomTestResultMatcher(), ServicePriority.High);
        }
    }
}
