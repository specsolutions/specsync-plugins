using FluentAssertions;
using Newtonsoft.Json.Linq;
using SpecSync.Configuration;
using SpecSync.Parsing;
using SpecSync.Plugin.PostmanTestSource.Projects;
using SpecSync.Projects;
using SpecSync.Synchronization;

namespace SpecSync.Plugin.PostmanTestSource.Tests;

[TestClass]
public class PostmanTestUpdaterTests : TestBase
{
    private readonly PostmanProject _postmanProject;

    public PostmanTestUpdaterTests()
    {
        var loader = new PostmanCollectionLoader(Parameters);
        var project = loader.LoadProject(new BddProjectLoaderArgs(SynchronizationContextStub.Object, new LocalConfiguration(), Path.GetTempPath()));
        _postmanProject = (PostmanProject)project;
    }

    private PostmanTestItem GetTest(string folder, string name)
    {
        var folderItem = _postmanProject.FolderItems.Should().Contain(f => f.Name == folder).Subject;
        return folderItem.Tests.Should().Contain(t => t.Name == name).Subject;
    }

    private void AssertTestCaseId(PostmanTestItem testItem)
    {
        testItem.Metadata.DocumentationContent.UpdatedSourceCode.Should().Contain("## Metadata\n\n- tc: 1234");
        var updatedCollection = LastPayload.Should().BeOfType<JObject>().Subject;
        updatedCollection.ToString().Should().Contain("## Metadata\\n\\n- tc: 1234");
    }

    private PostmanTestUpdater CreateSut() => new(PostmanApi, Parameters);

    [TestMethod]
    public void Should_update_test_case_id_for_request_without_metadata()
    {
        var sut = CreateSut();
        var testItem = GetTest("Request Methods", "GET Request");

        sut.SetTestCaseLink(testItem, new TestCaseLink(TestCaseIdentifier.CreateExistingFromNumericId(1234), "tc"));
        sut.Flush();

        AssertTestCaseId(testItem);
    }

    [TestMethod]
    public void Should_update_test_case_id_for_request_with_metadata()
    {
        var sut = CreateSut();
        var testItem = GetTest("Request Methods", "POST Raw Text");

        sut.SetTestCaseLink(testItem, new TestCaseLink(TestCaseIdentifier.CreateExistingFromNumericId(1234), "tc"));
        sut.Flush();

        AssertTestCaseId(testItem);
    }


    [TestMethod]
    public void Should_update_test_case_id_for_folder()
    {
        var sut = CreateSut();
        var testItem = GetTest("Postman Echo", "Headers");

        sut.SetTestCaseLink(testItem, new TestCaseLink(TestCaseIdentifier.CreateExistingFromNumericId(1234), "tc"));
        sut.Flush();

        AssertTestCaseId(testItem);
    }
}