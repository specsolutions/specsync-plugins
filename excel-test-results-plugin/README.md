# Excel Test Results SpecSync Plugin: SpecSync.Plugin.ExcelTestResults

This plugin can be used to provide test results for Ghekin scenarios from an Excel file. This might 
be useful when manual test executions have to be recorded.

*You can find more information about the SpecSync sample plugins in the [repository page](https://github.com/specsolutions/specsync-sample-plugins#readme).*

* Plugin package: [SpecSync.Plugin.ExcelTestResults](https://www.nuget.org/packages/SpecSync.Plugin.ExcelTestResults)
* Plugin source: [SpecSync.Plugin.MsTestTestSource](SpecSync.Plugin.MsTestTestSource)
* Sample project: [SampleProject](SampleProject)

It can match the rest results to the local test cases (scenarios) with:

* Feature name
* Feature file name (without folder names)
* Scenario name
* Test Case ID

For that you need to have an Excel file that contains some of the columns below (the columns can be renamed, see below).

* `Feature`: The column that contains the feature name. Optional, should be specified when scenario names are not globally unique and the `ID` column is not specified.
* `Feature File`: The column that contains the feature file name. Optional, should be specified when scenario names are not globally unique the `ID` column is not specified.
* `Scenario`: The column that contains the scenario name. Optional, must be specified when the `ID` column is not specified.
* `ID`: The column that contains the Test Case ID. Optional, must be specified when `Scenario` column is not specified.
* `Test Name`: The column that contains the name of the test execution (displayed in Azure DevOps). Optional, the scenario name is used as a Test Name if there is no such column or if the cell is empty.
* `Outcome`: The column that contains the outcome (Passed, Failed, NotExecuted). Mandatory.
* `Error`: The column that contains the error message. Optional, no error message is recoded if not specified.

By default the first sheet of the Excel file is processed, but your can specify the sheet name as a paramter to select another sheet:

```
"plugins": [
  {
    "packageId": "SpecSync.Plugin.ExcelTestResults",
    [...]
    "parameters": {
      "TestResultSheetName": "My Sheet"
    }
  }
]
```

You can use different column names, but in that case you need to configure the names by specifying any of the following plugin parameters:

* `FeatureColumnName`
* `FeatureFileColumnName`
* `ScenarioColumnName`
* `OutcomeColumnName`
* `TestCaseIdColumnName`
* `TestNameColumnName`
* `ErrorMessageColumnName`


In order to use the plugin, you have to specify `Excel` for the `--testResultFileFormat` (or `-f`) command line option:

```
dotnet specsync publish-test-results -r ExcelTestResults.xlsx -f Excel
```

## Implementation notes

The plugin finds the matching scenarios by case-sensitive equality. You can define different matching rules by changing the `ExcelTestResultMatcher` class.

The plugin loads the test result and the error message from the Excel file. You can load additional test result data (e.g. duration or step results) by extending the `ExcelTestResultLoader` class.
