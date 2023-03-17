# Excel Test Source SpecSync Plugin: SpecSync.Plugin.ExcelTestSource

This plugin can be used to synchronize a local test cases from Excel file using the format that Azure DevOps uses when you export Test Cases to CSV. 

*You can find more information about the SpecSync sample plugins in the [repository page](https://github.com/specsolutions/specsync-sample-plugins#readme).*

* Plugin package: [SpecSync.Plugin.ExcelTestSource](https://www.nuget.org/packages/SpecSync.Plugin.ExcelTestSource)
* Plugin source: [SpecSync.Plugin.ExcelTestSource](SpecSync.Plugin.ExcelTestSource)
* Sample project: [SampleProject](SampleProject)

If the Excel contains new Test Case rows (where the ID cell is empty), SpecSync will create a new Test Case and 
set the ID cell value with the ID of the created Test Case.

The sample demonstrates how to read and update custom test case sources.

The plugins that override local test source require a SpecSync Enterprise license to run. Please [contact us](https://specsolutions.gitbook.io/specsync/contact/specsync-support) to get an evaluation license that you can use to try out this plugin.
