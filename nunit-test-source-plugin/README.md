# NUnit Test Source SpecSync Plugin: SpecSync.Plugin.NUnitTestSource

Allows synchronizing "C# NUnit Tests" and publish results from TRX result files.

*You can find more information about the SpecSync sample plugins in the [repository page](https://github.com/specsolutions/specsync-sample-plugins#readme).*

* Plugin package: [SpecSync.Plugin.NUnitTestSource](https://www.nuget.org/packages/SpecSync.Plugin.NUnitTestSource)
* Plugin source: [SpecSync.Plugin.NUnitTestSource](SpecSync.Plugin.NUnitTestSource)
* Sample project: [SampleProject](SampleProject)

The plugin processes the C# files in the configured folder tree and searches for *NUnit* test methods, like:

```
[Test]
[Category("MyCategory")]
public void OnePassingTest()
{
    ...
}
```

These test methods are the potential local test cases to be synchronized. 

Once the methods are linked to a newly created Azure DevOps Test Case, the Test Case ID is inserted into the 
C# file as a `[Category]` attribute using a "SpecSync tag" (see below).


#### Specifying tags for SpecSync

* Specify tags for the tests, using the `[Category]` attribute:
    * `[Category("my_tag")]` or `[Category("story:123")]`


