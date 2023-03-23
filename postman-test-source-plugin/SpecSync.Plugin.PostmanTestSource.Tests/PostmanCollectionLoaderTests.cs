using FluentAssertions;
using SpecSync.Configuration;
using SpecSync.Plugin.PostmanTestSource.Projects;
using SpecSync.Projects;

namespace SpecSync.Plugin.PostmanTestSource.Tests;

[TestClass]
public class PostmanCollectionLoaderTests : TestBase
{
    [TestMethod]
    public void Should_load_folder_and_test_items_from_collection()
    {
        var sut = new PostmanCollectionLoader(Parameters);

        var project = sut.LoadProject(new BddProjectLoaderArgs(SynchronizationContextStub.Object, new LocalConfiguration(), Path.GetTempPath()));
        project.Should().NotBeNull();
        project.LocalTestContainerFiles.Should().HaveCountGreaterThan(0);
        var postmanProject = project as PostmanProject;
        postmanProject.Should().NotBeNull();
        postmanProject!.FolderItems.Should().Contain(c => c.Tests.Any());
        // there is at least one test that is not a direct request, but a folder
        var rootFolderItem = postmanProject.FolderItems.First();
        rootFolderItem.Tests.Should().HaveCountGreaterThanOrEqualTo(1);
        rootFolderItem.Tests.Should().Contain(t => t.ModelItem.Request == null);
    }

    [TestMethod]
    public void Should_recognize_test_by_item_name()
    {
        Parameters.TestNameRegex = @"^Test(?<id>\d+)?:";
        Parameters.CheckParameters("plugin");
        var sut = new PostmanCollectionLoader(Parameters);

        var postmanProject = sut.LoadProject(new BddProjectLoaderArgs(SynchronizationContextStub.Object, new LocalConfiguration(), Path.GetTempPath())) as PostmanProject;
        postmanProject.Should().NotBeNull();

        var tests = postmanProject!.FolderItems.SelectMany(f => f.Tests);
        tests.Should().Contain(t => t.Name == "Test: Authentication Methods");
    }

    [TestMethod]
    public void Should_recognize_linked_test_by_item_name()
    {
        Parameters.TestNameRegex = @"^Test(?<id>\d+)?:";
        Parameters.CheckParameters("plugin");
        var sut = new PostmanCollectionLoader(Parameters);

        var postmanProject = sut.LoadProject(new BddProjectLoaderArgs(SynchronizationContextStub.Object, new LocalConfiguration(), Path.GetTempPath())) as PostmanProject;
        postmanProject.Should().NotBeNull();
        
        var tests = postmanProject!.FolderItems.SelectMany(f => f.Tests);
        var test = tests.Should().Contain(t => t.Name == "Test209: Auth: Digest").Subject;
        var testCaseLink = PostmanFolderItemParser.GetTestCaseLinkFromMetadata(test.Metadata, test.ParentMetadata, Configuration);
        testCaseLink.Should().NotBeNull();
        testCaseLink.TestCaseId.ToString().Should().Be("209");
    }

    [TestMethod]
    public void Should_recognize_linked_test_by_item_documentation()
    {
        Parameters.TestDocumentationRegex = @"\badoid=(?<id>\d+)\b";
        Parameters.CheckParameters("plugin");
        var sut = new PostmanCollectionLoader(Parameters);

        var postmanProject = sut.LoadProject(new BddProjectLoaderArgs(SynchronizationContextStub.Object, new LocalConfiguration(), Path.GetTempPath())) as PostmanProject;
        postmanProject.Should().NotBeNull();
        
        var tests = postmanProject!.FolderItems.SelectMany(f => f.Tests);
        var test = tests.Should().Contain(t => t.Name == "POST Server events").Subject;
        var testCaseLink = PostmanFolderItemParser.GetTestCaseLinkFromMetadata(test.Metadata, test.ParentMetadata, Configuration);
        testCaseLink.Should().NotBeNull();
        testCaseLink.TestCaseId.ToString().Should().Be("212");
    }
}