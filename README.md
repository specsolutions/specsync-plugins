# Sample SpecSync for AzureDevOps plugins

_Note: Plugins are supported in SpecSync v3.1 or later_

In the project that would like to use the plugin, the plugin assembly path (and optionally plugin parameters)
have to be configured in `specsync.josn`. These can also be specified in parent config files as well.

```
  "toolSettings": {
    "plugins": [
      {
        "assemblyPath": "plugin-path\MyCustomTestResultMatch.SpecSyncPlugin.dll",
        "parameters": {
          "key1": "value1"
        }
      }
    ] 
  },
```

## test-result-match-plugin

This plugin shows how to enable publishing test results for a test framework that can 
produce TRX output, but it is not supported by SpecSync yet.

If the tool can produce TRX output, the plugin only needs to implement a test result matcher.
The matcher should tell whether a test result belongs to a particular scenario or not. For that 
you can build up conditions that use the `name`, the `className`, the `methodName` and other fields 
of the test result.

For diagnosing the plugin and the test results loaded from the TRX file, you can call SpecSync with the
`-v` option.

As an example, the sample plugin finds the test results if their `className` ends with the feature name and 
if the `name` is exacly the scenario name. Other matchers often use regular expressions as well.

## excel-test-results-plugin

The plugin shows how to publish test results for synchronized scenarios from an Excel file.

The current implementation of the plugin can match

* Feature name
* Feature file name (without folder names)
* Scenario name
* Test Case ID

For that, you need to provide an Excel result specification (see `ExcelResultSpecification` class) in your customized version of the plugin (check the `ExcelTestResultsPlugin` class), 
or in the specsync.json configuration file with the following options:

* `TestResultSheetName`: The sheet name that contains the test results. Optional, uses the first sheet if not specified.
* `FeatureColumnName`: The column name that contains the feature name. Optional, should be specified when scenario names are not globally unique and `TestCaseIdColumnName` is not specified.
* `FeatureFileColumnName`: The column name that contains the feature file name. Optional, should be specified when scenario names are not globally unique and `TestCaseIdColumnName` is not specified.
* `ScenarioColumnName`: The column name contains the scenario name. Optional, must be specified when `TestCaseIdColumnName` is not specified.
* `OutcomeColumnName`: The column name contains the outcome (Passed, Failed, NotExecuted). Mandatory.
* `TestCaseIdColumnName`: The column name contains the Test Case ID. Optional, must be specified when `ScenarioColumnName` is not specified.
* `TestNameColumnName`: The column name contains the name (displayed in Azure DevOps). Optional, the first column is used if not specified.
* `ErrorMessageColumnName`: The column name contains the error message. Optional, no error message is recoded if not specified.

A sample configuration in the specsync.json file would look like this:

```
"plugins": [
  {
    "assemblyPath": "<path-to-plugin>\\ExcelTestResults.SpecSyncPlugin.dll",
    "parameters": {
      "OutcomeColumnName": "Result",
      "FeatureColumnName": "Feature",
      "ScenarioColumnName": "Scenario",
      "TestNameColumnName": "Test Name",
      "ErrorMessageColumnName": "Error"
    }
  }
]
```

In order to use the plugin, you have to specify `Excel` for the `--testResultFileFormat` (or `-f`) command line option:

```
dotnet specsync publish-test-results -r ExcelTestResults.xlsx -f Excel
```

Note: The plugin finds the matching scenarios by case-sensitive equality. You can define different matching rules by changing the `ExcelTestResultMatcher` class.

Note: The plugin loads the test result and the error message from the Excel file. You can load additional test result data (e.g. duration or step results) by extending the `ExcelTestResultLoader` class.

## custom-test-source-plugin

This plugin shows how to use SpecSync to synchronize a custom local test source. The local test source 
is normally a scenario in a feature file, but you can implement different local test sources as well.

This plugin makes a normal MsTest test to be available as a test source for SpecSync. For that it provides 

* a custom `IBddProject` that lists the test classes and test methods from an assembly,
* a custom `ILocalTestCaseContainerParser` that parses the `TestCategory` attributes on the test method to get the test case ID and the other related work items,
* a custom `ITestRunnerResultMatcher` to connect back the test results from a TRX file to the parsed test method.

The plugins that override local test source require a SpecSync Enterprise license to run. Please [contact us](https://specsolutions.gitbook.io/specsync/contact/specsync-support) to get an evaluation license that you can use to try out this plugin.
