using System.Reflection;
using SpecSync.Plugins;
using SpecSync.Synchronization;

namespace SpecSync.Plugin.OnlyPublishTestResults;

public static class CompatibilityExtensions
{
    public static ISynchronizationContext GetSynchronizationContext(this ServiceArgs args)
    {
        var contextProperty = args.GetType().GetProperty("SynchronizationContext",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        return (ISynchronizationContext)contextProperty!.GetValue(args);
    }
}