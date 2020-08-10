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
`--diag` option.

As an example, the sample plugin finds the test results if their `className` ends with the feature name and 
if the `name` is exacly the scenario name. Other matchers often use regular expressions as well.
