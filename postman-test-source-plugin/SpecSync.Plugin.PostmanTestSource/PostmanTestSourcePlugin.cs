using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using SpecSync.Configuration;
using SpecSync.Plugin.PostmanTestSource;
using SpecSync.Plugins;

[assembly: SpecSyncPlugin(typeof(PostmanTestSourcePlugin))]

namespace SpecSync.Plugin.PostmanTestSource;

public class PostmanTestSourcePlugin : ISpecSyncPlugin
{
    public const string InternalTagPrefix = "__plugin:";
    public const string ManualTagName = InternalTagPrefix + "Manual";

    public string Name => "Postman Test Case Source";

    public void Initialize(PluginInitializeArgs args)
    {
        args.Tracer.LogVerbose($"Initializing '{Name}' plugin...");

        var fieldUpdaterColumnParameters = LoadParameters(args);

        args.ServiceRegistry.BddProjectLoaderProvider
            .Register(new ExcelFolderProjectLoader());
        args.ServiceRegistry.LocalTestCaseContainerParserProvider
            .Register(new ExcelTestCaseSourceParser(fieldUpdaterColumnParameters));
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

        foreach (var fieldUpdaterColumnParameter in fieldUpdaterColumnParameters)
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

    private List<FieldUpdaterColumnParameter> LoadParameters(PluginInitializeArgs args)
    {
        if (!args.Parameters.TryGetValue("fieldUpdateColumns", out var fieldUpdateColumnObj))
            return new List<FieldUpdaterColumnParameter>();

        if (fieldUpdateColumnObj is not JArray fieldUpdateColumnArray)
            throw new SpecSyncConfigurationException("The 'fieldUpdateColumns' must contain an array.");

        var result = fieldUpdateColumnArray.ToObject<List<FieldUpdaterColumnParameter>>()
            ?? new List<FieldUpdaterColumnParameter>();

        foreach (var parameter in result)
        {
            if (string.IsNullOrEmpty(parameter.ColumnName))
                throw new SpecSyncConfigurationException("The 'fieldUpdateColumns/columnName' must be specified.");
        }

        return result;
    }
}