# Excel Test Source SpecSync Plugin: SpecSync.Plugin.ExcelTestSource

This plugin can be used to synchronize a local test cases from Excel file using the format that Azure DevOps uses when you export Test Cases to CSV. 

*You can find more information about the SpecSync sample plugins in the [repository page](https://github.com/specsolutions/specsync-sample-plugins#readme).*

* Plugin package: [SpecSync.Plugin.ExcelTestSource](https://www.nuget.org/packages/SpecSync.Plugin.ExcelTestSource)
* Plugin source: [SpecSync.Plugin.ExcelTestSource](SpecSync.Plugin.ExcelTestSource)
* Sample project: [SampleProject](SampleProject)

The Test Cases are loaded from the Excel from a hierarchical structure: a Test Case row (step fields empty) and additional test step rows (Test Case fields empty).

In order to load the Test Cases from excel, the plugin uses the following columns in the Excel file:
* Test Case ID (default column name: `ID`), mandatory column.
* Test Case Title (default column name: `Title`), mandatory column.
* Test Case Step Index (default column name: `Test Step`), mandatory column.
* Test Case Step Action (default column name: `Step Action`), mandatory column.
* Test Case Step Expected Result (default column name: `Step Expected`), optional column.
* Test Case Tags (default column name: `Tags`), optional column.
* Test Case Description (default column name: `Description`), optional column.
* Test Case Automation Status (default column name: `Automation Status`), optional column.
* Test Case Automated Test Name (default column name: `Automated Test Name`), optional column.

The default column names can be changed using plugin parameters. The following example shows how to rename all columns:

```
    "plugins": [
      {
        "packageId": "SpecSync.Plugin.ExcelTestSource",
        [...]
        "parameters": {
          "TestCaseIdColumnName": "My ID",
          "TitleColumnName": "My Title",
          "TestStepColumnName": "My Test Step",
          "TestStepActionColumnName": "My Step Action",
          "TestStepExpectedColumnName": "My Step Expected",
          "TagsColumnName": "My Tags",
          "DescriptionColumnName": "My Description",
          "AutomationStatusColumnName": "My Automation Status",
          "AutomatedTestNameColumnName": "My Automated Test Name",
        }
      }
    ]
```


All other columns can be automatically updated to fields of the Test Case. In order to do that, you need to add an item to the `fieldUpdateColumns` plugin parameter setting, like

```
    "plugins": [
      {
        "packageId": "SpecSync.Plugin.ExcelTestSource",
        [...]
        "parameters": {
          "fieldUpdateColumns": [
            {
              "columnName": "Priority",
              "fieldName": "Priority" // can be omitted if the same as 'columnName'
            }
          ]
        }
      }
    ]
```

If the automatic update is not enough (e.g. because you need to transform the value), you can also convert the column values to normal tags and then use the "updateFields" feature to save them to fields. For this, the `TagNamePrefix` setting has to be specified:


```
    "plugins": [
      {
        "packageId": "SpecSync.Plugin.ExcelTestSource",
        [...]
        "parameters": {
          "fieldUpdateColumns": [
            {
              "columnName": "Priority",
              "tagNamePrefix": "priority:" // generates tags, like `@priority:high`
            }
          ]
        }
      }
    ]
```

If the Excel contains new Test Case rows (where the ID cell is empty), SpecSync will create a new Test Case and 
set the ID cell value with the ID of the created Test Case.

The plugins that override local test source require a SpecSync Enterprise license to run. Please [contact us](https://specsolutions.gitbook.io/specsync/contact/specsync-support) to get an evaluation license that you can use to try out this plugin.
