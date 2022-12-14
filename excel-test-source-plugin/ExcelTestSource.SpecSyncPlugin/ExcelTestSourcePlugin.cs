using System;
using System.Linq;
using ExcelTestSource.SpecSyncPlugin;
using SpecSync.Plugins;

[assembly: SpecSyncPlugin(typeof(ExcelTestSourcePlugin))]

namespace ExcelTestSource.SpecSyncPlugin;

public class ExcelTestSourcePlugin : ISpecSyncPlugin
{
    public const string InternalTagPrefix = "__plugin:";
    public const string ManualTagName = InternalTagPrefix + "Manual";

    public string Name => "Excel Test Case Source";

    public void Initialize(PluginInitializeArgs args)
    {
        args.Tracer.LogVerbose($"Initializing '{Name}' plugin...");
        args.ServiceRegistry.BddProjectLoaderProvider
            .Register(new ExcelFolderProjectLoader());
        args.ServiceRegistry.LocalTestCaseContainerParserProvider
            .Register(new ExcelTestCaseSourceParser());
        args.ServiceRegistry.LocalTestCaseAnalyzerProvider
            .Register(new ExcelTestCaseAnalyzer());

        // configure automation condition based on the "Automation Status" column
        args.Configuration.Synchronization.Automation.Condition =
            string.IsNullOrEmpty(args.Configuration.Synchronization.Automation.Condition)
                ? $"not @{ManualTagName}"
                : $"({args.Configuration.Synchronization.Automation.Condition}) and not @{ManualTagName}";
        if (string.IsNullOrEmpty(args.Configuration.Synchronization.Automation.AutomatedTestType))
            args.Configuration.Synchronization.Automation.AutomatedTestType = "Unknown";

        args.Configuration.Customizations.IgnoreNotSupportedLocalTags.Enabled = true;
        args.Configuration.Customizations.IgnoreNotSupportedLocalTags.NotSupportedTags =
            (args.Configuration.Customizations.IgnoreNotSupportedLocalTags.NotSupportedTags ?? Array.Empty<string>())
            .Concat(new[] { $"{InternalTagPrefix}*" }).ToArray();
    }
}