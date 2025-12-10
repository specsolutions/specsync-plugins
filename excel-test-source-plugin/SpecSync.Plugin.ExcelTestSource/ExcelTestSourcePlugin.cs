using SpecSync.Configuration;
using SpecSync.Plugin.ExcelTestSource;
using SpecSync.Plugins;

[assembly: SpecSyncPlugin(typeof(ExcelTestSourcePlugin))]

namespace SpecSync.Plugin.ExcelTestSource;

public class ExcelTestSourcePlugin : ISpecSyncPlugin
{
    public const string InternalTagPrefix = "__plugin:";
    public const string ManualTagName = InternalTagPrefix + "Manual";

    public string Name => "Excel Test Case Source";

    public void Initialize(PluginInitializeArgs args)
    {
        args.Tracer.LogVerbose($"Initializing '{Name}' plugin...");

        var parameters = args.GetParametersAs<ExcelTestSourceParameters>();
        parameters.Verify();

        args.ServiceRegistry.ProjectLoaderProvider
            .Register(new ExcelFolderProjectLoader(), ServicePriority.High);
        args.ServiceRegistry.SourceDocumentParserProvider
            .Register(new ExcelSourceDocumentParser(parameters));
        args.ServiceRegistry.LocalArtifactAnalyzerProvider
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
            args.Configuration.Customizations.IgnoreNotSupportedLocalTags.NotSupportedTags
            .Concat([$"{InternalTagPrefix}*"]).ToArray();

        foreach (var fieldUpdaterColumnParameter in parameters.FieldUpdateColumns)
        {
            if (fieldUpdaterColumnParameter.TagNamePrefix == null)
            {
                args.Configuration.Synchronization.FieldUpdates.Add(fieldUpdaterColumnParameter.ActualFieldName,
                    new FieldUpdateValueConfiguration
                    {
                        Condition = $"@{fieldUpdaterColumnParameter.GeneratedTagNamePrefix}*",
                        Value = "{1}"
                    });
            }
        }
    }
}