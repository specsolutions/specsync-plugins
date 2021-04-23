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

## custom-test-source-plugin

This plugin shows how to use SpecSync to synchronize a custom local test source. The local test source 
is normally a scenario in a feature file, but you can implement different local test sources as well.

This plugin makes a normal MsTest test to be available as a test source for SpecSync. For that it provides 

* a custom `IBddProject` that lists the test classes and test methods from an assembly,
* a custom `ILocalTestCaseContainerParser` that parses the `TestCategory` attributes on the test method to get the test case ID and the other related work items,
* a custom `ITestRunnerResultMatcher` to connect back the test results from a TRX file to the parsed test method.

The plugins that override local test source require a SpecSync Enterprise license to run. Please [contact us](https://specsolutions.gitbook.io/specsync/contact/specsync-support) to get an evaluation license that you can use to try out this plugin.
