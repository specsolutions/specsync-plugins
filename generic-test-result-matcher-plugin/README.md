# Generic Test Result Matcher SpecSync Plugin: SpecSync.Plugin.GenericTestResultMatcher

A SpecSync plugin that can be used to override test result matching rules.

*You can find more information about the SpecSync sample plugins in the [repository page](https://github.com/specsolutions/specsync-sample-plugins#readme).*

* Plugin package: [SpecSync.Plugin.GenericTestResultMatcher](https://www.nuget.org/packages/SpecSync.Plugin.GenericTestResultMatcher)
* Plugin source: [SpecSync.Plugin.GenericTestResultMatcher](SpecSync.Plugin.GenericTestResultMatcher)
* Sample project: [SampleProject](SampleProject)

The matching rules have to be provided as pluign parameter as regular expressions
that might include special placeholders to refer to the local test case details (see list below).

```
"plugins": [
  {
    "packageId": "SpecSync.Plugin.GenericTestResultMatcher",
    [...]
    "parameters": {
      "Name": "^{local-test-case-name}$"
    }
  }
]
```

The following parameters can be used:

* `Name` - matches the test result "name" parameter
* `ClassName` - matches the test result "className" or "classname" parameter
* `MethodName` - matches the test result "methodName" parameter (TRX only)
* `StdOut` - matches the test result output
* `TestResultProperties` - matches the test result custom properties, see example below

In order to see the "name", "className", "methodName" or the custom properties you can run 
the SpecSync "publish-test-results" command with additional `-v --diagCategories TestResult`. 

For testing it is also recommended to use the `--dryRun` option that collects all information, but does not publish the results.

The following placeholeds can be used:

* `{local-test-case-name}` - the name of the local test case (name of the scenario)
* `{local-test-case-container-name}` - the name of the local test case container (name of the feature)
* `{local-test-case-container-filename}` - the file name of the source file with extension
* `{test-case-id}` - the ID of the test case, see example below

In some cases the test result also contains the Test Case ID that could be used for matching. Assuming the Test Case ID is added to the result as a custom parameter `test_case_id`, the configuration could be the following:

```
"plugins": [
  {
    "packageId": "SpecSync.Plugin.GenericTestResultMatcher",
    [...]
    "parameters": {
      "TestResultProperties": {
        "test_case_id": "^{test-case-id}$"
      }
    }
  }
]
```
