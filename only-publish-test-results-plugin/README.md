# Only Publish Test Results Plugin: SpecSync.Plugin.OnlyPublishTestResults

This plugin can be used in cases when you would like to publish test results with SpecSync for Test Cases that were not synchronized by SpecSync, but created manually or with other tools.


*You can find more information about the SpecSync sample plugins in the [repository page](https://github.com/specsolutions/specsync-plugins#readme).*

* Plugin package: [SpecSync.Plugin.OnlyPublishTestResults](https://www.nuget.org/packages/SpecSync.Plugin.OnlyPublishTestResults)
* Plugin source: [SpecSync.Plugin.OnlyPublishTestResults](SpecSync.Plugin.OnlyPublishTestResults)
* Sample project: [SampleProject](SampleProject)

Normally SpecSync requires the local test case sources (e.g. feature files) as well for publishing test results, because the Test Case ID of a particular executed test is obtained from that. There are cases however, when the test results contain the Test Case ID and the test result should be just published to the referred Test Case. 

This plugin can be used to publish test results in this situation.

### Prerequisites & Setup

1. The test results must contain the Test Case ID as a test result property. Test result properties are set for example by the `<property>` element in JUnit XML results, but the [SpecSync.Plugin.ExcelTestResults](https://github.com/specsolutions/specsync-plugins/tree/main/excel-test-results-plugin) also sets the properties based on the Excel columns.
2. The plugin can only be used for `publish-test-results` SpecSync command. For all other commands (e.g. `push`) it fails. In order to use the same configuration file for other commands, you either need to remove the plugin or set `local/projectType` to `folder` or `projectFile`.
3. The Test Case ID can be set to a result property of any name, but this name has to be specified as a plugin parameter `TestCaseIdPropertyName`.

The property can contain the ID directly (e.g. `1234`) or in a prefixed form (e.g. `tc:1234`). When the prefixed form is used, the `synchronization/testCaseTagPrefix` and the `synchronization/tagPrefixSeparators` settings are considered. See example below.

The following example shows how this plugin can be used to publish test results from an Excel file if it contains `TestCaseId` column:


```
    "plugins": [
      {
        "packageId": "SpecSync.Plugin.OnlyPublishTestResults",
        [...]
        "parameters": {
          "TestCaseIdPropertyName": "TestCaseId" // specifying the property name that contains the Test Case ID
        }
      },
      {
        "packageId": "SpecSync.Plugin.ExcelTestResults",
        "packageVersion": "1.3.0",
        "parameters": {
          "TestCaseIdColumnName": "TestCaseId"
        }
      }
    ]
```

The second example can be used to load a JUnit XML test result file, that includes a `test_case` property with a prefixed version of the Test Case ID. In this example the [SpecSync.Plugin.GenericTestResultMatcher](https://github.com/specsolutions/specsync-plugins/tree/main/generic-test-result-matcher-plugin) is used to match the results to the Test Case.

The JUnit XML test result file looks like this.

```
<testsuite name="my test run">
    <testcase name="Sample scenario" classname="My Feature" status="passed">
        <properties>
            <property name="test_case" value="TC-32249" />
        </properties>
    </testcase>
</testsuite>
```

In order to process this test result file, the following configuration has to be used.

```
{
  "$schema": "https://schemas.specsolutions.eu/specsync4azuredevops-config-latest.json",
  "compatibilityVersion": "1.0",

  "toolSettings": {
    "plugins": [
      {
        "packageId": "SpecSync.Plugin.OnlyPublishTestResults",
        "packageVersion": "1.0.0",

        "parameters": {
          "TestCaseIdPropertyName": "test_case"
        }
      },
      {
        "packageId": "SpecSync.Plugin.GenericTestResultMatcher",
        "packageVersion": "1.2.0",
        "parameters": {
          "TestResultProperties": {
            "test_case": "^TC-{test-case-id}$" // the 'test_case' parameter should contain the Test Case ID with the 'TC-' prefix.
          }
        }
      }
    ]
  },
  "remote": {
    "projectUrl": "https://dev.azure.com/specsync-demo/specsync-plugins-demo"
  },
  "synchronization": {
    "testCaseTagPrefix": "TC",
    "tagPrefixSeparators": ["-", ":"]
  }
}
```