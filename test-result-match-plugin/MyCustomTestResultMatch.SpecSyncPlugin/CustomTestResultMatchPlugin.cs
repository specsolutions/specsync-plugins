using System;
using MyCustomTestResult.SpecSyncPlugin;
using MyCustomTestResultMatch.SpecSyncPlugin;
using SpecSync.AzureDevOps.Synchronizer.Plugins;

[assembly: SpecSyncPlugin(typeof(CustomTestResultMatchPlugin))]

namespace MyCustomTestResultMatch.SpecSyncPlugin
{
    public class CustomTestResultMatchPlugin : ISpecSyncPlugin
    {
        public string Name => "Custom Test Result Support";

        public void Initialize(PluginInitializeArgs args)
        {
            args.Tracer.LogDebug("Initializing custom plugin...");
            args.ServiceRegistry.TestResultMatcher.Register(new CustomTestResultMatcher(), ServicePriority.High);
        }
    }
}
