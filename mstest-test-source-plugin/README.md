# MsTest Test Source SpecSync Plugin: SpecSync.Plugin.MsTestTestSource

Allows synchronizing "C# MsTest Tests" and publish results from TRX result files.

*You can find more information about the SpecSync sample plugins in the [repository page](https://github.com/specsolutions/specsync-sample-plugins#readme).*

* Plugin package: [SpecSync.Plugin.MsTestTestSource](https://www.nuget.org/packages/SpecSync.Plugin.MsTestTestSource)
* Plugin source: [SpecSync.Plugin.MsTestTestSource](SpecSync.Plugin.MsTestTestSource)
* Sample project: [SampleProject](SampleProject)

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


