# Sample SpecSync for AzureDevOps plugins

_Note: The `main` branch contains sample plugins for SpecSync v3.3. In order to find samples for SpecSync v3.2, please use the branch `specsync-v3.2`._

In the project that would like to use the plugin, the plugin assembly path (and optionally plugin parameters)
have to be configured in `specsync.json`. These can also be specified in parent config files as well.

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

You can find more information about SpecSync plugins in the [SpecSync documentation](https://speclink.me/specsync-plugins).

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

## mstest-test-source-plugin

Allows synchronizing "C# MsTest Tests" and publish results from TRX result files.

* Plugin source: https://github.com/specsolutions/specsync-sample-plugins/tree/main/mstest-test-source-plugin/SpecSync.Plugin.MsTestTestSource
* Sample project: https://github.com/specsolutions/specsync-sample-plugins/tree/main/mstest-test-source-plugin/SampleProject

The plugin processes the C# files in the configured folder tree and searches for *MsTest* test methods, like:

```
[TestMethod]
[TestCategory("MyCategory")]
public void OnePassingTest()
{
    ...
}
```

These test methods are the potential local test cases to be synchronized. 

Once the methods are linked to a newly created Azure DevOps Test Case, the Test Case ID is inserted into the 
C# file as a `[TestCategory]` attribute using a "SpecSync tag" (see below).


#### Specifying tags for SpecSync

* Specify tags for the tests, using the `[TestCategory]` attribute:
    * `[TestCategory("my_tag")]` or `[TestCategory("story:123")]`


## custom-test-source-plugin

This plugin shows how to use SpecSync to synchronize a custom local test source. The local test source 
is normally a scenario in a feature file, but you can implement different local test sources as well.

This plugin makes a normal MsTest test to be available as a test source for SpecSync. For that it provides 

* a custom `IBddProject` that lists the test classes and test methods from an assembly,
* a custom `ILocalTestCaseContainerParser` that parses the `TestCategory` attributes on the test method to get the test case ID and the other related work items,
* a custom `ITestRunnerResultMatcher` to connect back the test results from a TRX file to the parsed test method.

The plugins that override local test source require a SpecSync Enterprise license to run. Please [contact us](https://specsolutions.gitbook.io/specsync/contact/specsync-support) to get an evaluation license that you can use to try out this plugin.

## scenario-outline-as-normal-test-case-format-plugin

_Note: This plugin is supported in SpecSync v3.3 or later_

This plugin shows how to use SpecSync to drastically change the format of 
Test Cases synchronized from scenarios. 

This plugin changes the format of Test Cases synchronized from scenario 
outlines. Normally from scenario outlines SpecSync synchronizes a parametrized 
Test Case with the different examples as parameters, but with this plugin a 
"normal" (non-parametrized) Test Case is created and the values from the 
different examples are added as extra steps. This is implemented by a custom
`ILocalTestCaseAnalyzer` implementation (`ScenarioOutlineAsNormalTestCaseGherkinAnalyzer`)
that changes the default behavior of the built-in `GherkinLocalTestCaseAnalyzer`
class.

The plugins that override local test case analyzer require a SpecSync 
Enterprise license to run. Please [contact us](https://specsolutions.gitbook.io/specsync/contact/specsync-support) 
to get an evaluation license that you can use to try out this plugin.

## excel-test-source-plugin

This plugin shows how to use SpecSync to synchronize a local test cases from Excel file using the format that 
Azure DevOps uses when you export Test Cases to CSV. 

If the Excel contains new Test Case rows (where the ID cell is empty), SpecSync will create a new Test Case and 
set the ID cell value with the ID of the created Test Case.

The sample demonstrates how to read and update custom test case sources.

The plugins that override local test source require a SpecSync Enterprise license to run. Please [contact us](https://specsolutions.gitbook.io/specsync/contact/specsync-support) to get an evaluation license that you can use to try out this plugin.
