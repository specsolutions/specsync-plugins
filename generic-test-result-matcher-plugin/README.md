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

* `Name` - matches to the test result "name" parameter
* `ClassName` - matches to the test result "className" or "classname" parameter
* `MethodName` - matches to the test result "methodName" parameter (TRX only)

In order to see the "name", "className" or the "methodName" you can run 
the SpecSync "publish-test-results" command with 
additional `-v --diagCategories TestResult`. 

For testing it is also recommended to use the `--dryRun` option that collects all information, but does not publish the results.

The following placeholeds can be used:

* `{local-test-case-name}` - the name of the local test case (name of the scenario)
* `{local-test-case-container-name}` - the name of the local test case container (name of the feature)
* `{local-test-case-container-filename}` - the file name of the source file with extension

