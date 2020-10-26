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
            args.Tracer.LogVerbose("Initializing custom plugin...");
            args.ServiceRegistry.TestResultMatcher.Register(new CustomTestResultMatcher(), ServicePriority.High);
        }
    }
}
