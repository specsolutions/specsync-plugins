using System;
using ExcelTestSource.SpecSyncPlugin;
using SpecSync.Plugins;

[assembly: SpecSyncPlugin(typeof(ExcelTestSourcePlugin))]

namespace ExcelTestSource.SpecSyncPlugin;

public class ExcelTestSourcePlugin : ISpecSyncPlugin
{
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
    }
}