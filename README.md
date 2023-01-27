# Sample SpecSync plugins

_Note: The `main` branch contains sample plugins for SpecSync v3.4. In order to find samples for SpecSync v3.3, please use the branch `specsync-v3.3`._

In the project that would like to use the plugin, the plugin package ID, version have to be specified in the SpecSync configuration file (`specsync.json`). 
Most of the plugins are published to nuget.org, so SpecSync will automatically load them, but you can 
also specify a custom NuGet package feed or simply a folder where the plugin package (.nupkg) file is stored.

The sample projects in the plugin folders show how to load the plugins from a local folder.

Some plugins can have plugin parameters as well, please check the plugin pages for the details.

```
  "toolSettings": {
    "plugins": [
      {
        "packageId": "SpecSync.Plugin.ExcelTestSource",
        "packageVersion": "1.0.0",
        "parameters": {
          "key1": "value1"
        }
      }
    ] 
  },
```

You can find more information about SpecSync plugins in the [SpecSync documentation](https://speclink.me/specsync-plugins).

Currently the following plugins are available. For more details about the plugin, click on the name.

* [SpecSync.Plugin.ExcelTestResults](excel-test-results-plugin): This plugin can be used to provide test results for Ghekin scenarios from an Excel file. This might 
be useful when manual test executions have to be recorded.
* [SpecSync.Plugin.ExcelTestSource](excel-test-source-plugin): This plugin can be used to synchronize a local test cases from Excel file using the format that Azure DevOps uses when you export Test Cases to CSV.
* [SpecSync.Plugin.MsTestTestSource](mstest-test-source-plugin): Allows synchronizing "C# MsTest Tests" and publish results from TRX result files.
* [SpecSync.Plugin.NUnitTestSource](nunit-test-source-plugin): Allows synchronizing "C# NUnit Tests" and publish results from TRX result files.
* [SpecSync.Plugin.ScenarioOutlinePerExamplesTestCase](scenario-outline-per-exampes-test-case-plugin): This plugin can be used to synchronize scenario outlines with multiple "Examples" blocks to multiple Test Cases (one for each Examples block).
* [SpecSync.Plugin.ScenarioOutlineAsNormalTestCase](scenario-outline-as-normal-test-case-format-plugin): A SpecSync plugin that synchronizes scenario outlines as normal (non-data-driven) Test Cases.
* [SpecSync.Plugin.GenericTestResultMatcher](generic-test-result-matcher-plugin): A SpecSync plugin that can be used to override test result matching rules.
