using System;
using ExcelTestResults.SpecSyncPlugin;
using SpecSync.AzureDevOps.Plugins;

[assembly: SpecSyncPlugin(typeof(ExcelTestResultsPlugin))]

namespace ExcelTestResults.SpecSyncPlugin
{
    public class ExcelTestResultsPlugin : ISpecSyncPlugin
    {
        public string Name => "Excel Test Result";

        public void Initialize(PluginInitializeArgs args)
        {
            args.Tracer.LogVerbose("Initializing custom plugin...");

            // The Excel structure specification can be specified in the plugin, like:
            //    var specification = new ExcelResultSpecification
            //    {
            //        OutcomeColumnName = "Result",
            //        FeatureColumnName = "Feature",
            //        ScenarioColumnName = "Scenario",
            //        TestNameColumnName = "Test Name"
            //    };
            // Or the configuration can be retrieved from the specsync.json configuration file, e.g.
            //    "plugins": [
            //      {
            //        "assemblyPath": "<path-to-plugin>\\ExcelTestResults.SpecSyncPlugin.dll",
            //        "parameters": {
            //          "OutcomeColumnName": "Result",
            //          "FeatureColumnName": "Feature",
            //          "ScenarioColumnName": "Scenario",
            //          "TestNameColumnName": "Test Name",
            //          "ErrorMessageColumnName": "Error"
            //        }
            //      }
            //    ]
            var specification = ExcelResultSpecification.FromPluginParameters(args.Parameters);

            args.ServiceRegistry.TestResultLoaderProvider.Register(new ExcelTestResultLoader(specification), ServicePriority.High);
            args.ServiceRegistry.TestResultMatcher.Register(new ExcelTestResultMatcher(specification), ServicePriority.High);
        }
    }
}
