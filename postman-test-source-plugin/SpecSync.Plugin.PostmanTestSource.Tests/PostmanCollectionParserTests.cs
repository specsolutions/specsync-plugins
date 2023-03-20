using FluentAssertions;
using Moq;
using SpecSync.Parsing;
using SpecSync.Plugin.PostmanTestSource.Postman.Models;
using SpecSync.Plugin.PostmanTestSource.Projects;
using SpecSync.Projects;

namespace SpecSync.Plugin.PostmanTestSource.Tests;

[TestClass]
public class PostmanCollectionParserTests : TestBase
{
    [TestMethod]
    public void Test1()
    {
        var sut = new PostmanCollectionParser();
        var projectStub = new Mock<IBddProject>();
        var folderCollection = new PostmanFolderItem("path", new List<IPostmanItem>
        {
            new PostmanTestItem(new Item
            {
                Name = "Test 1"
            }),
            new PostmanTestItem(new Item
            {
                Name = "Test 2"
            }),
        }, 
            new Collection
            {
                Info = new Info
                {
                    Name = "My collection",
                    Description = "Some text"
                }
            });
        var result = sut.Parse(new LocalTestCaseContainerParseArgs(projectStub.Object, folderCollection,
            SynchronizationContextStub.Object));
        result!.Should().NotBeNull();
        result.LocalTestCases.Should().HaveCount(2);

        result.Name.Should().Be("My collection");
        result.Description.Should().Be("Some text");
        result.LocalTestCases[0].Name.Should().Be("Test 1");
    }
}