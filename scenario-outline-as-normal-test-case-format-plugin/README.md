# Excel Test Source SpecSync Plugin: SpecSync.Plugin.ScenarioOutlineAsNormalTestCase

A SpecSync plugin that synchronizes scenario outlines as normal (non-data-driven) Test Cases.

*You can find more information about the SpecSync sample plugins in the [repository page](https://github.com/specsolutions/specsync-sample-plugins#readme).*

* Plugin package: [SpecSync.Plugin.ScenarioOutlineAsNormalTestCase](https://www.nuget.org/packages/SpecSync.Plugin.ScenarioOutlineAsNormalTestCase)
* Plugin source: [SpecSync.Plugin.ScenarioOutlineAsNormalTestCase](SpecSync.Plugin.ScenarioOutlineAsNormalTestCase)
* Sample project: [SampleProject](SampleProject)

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
