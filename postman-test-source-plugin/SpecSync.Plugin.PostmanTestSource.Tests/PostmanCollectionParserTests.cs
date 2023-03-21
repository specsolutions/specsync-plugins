using FluentAssertions;
using SpecSync.Parsing;
using SpecSync.Plugin.PostmanTestSource.Postman.Models;
using SpecSync.Plugin.PostmanTestSource.Projects;

namespace SpecSync.Plugin.PostmanTestSource.Tests;

[TestClass]
public class PostmanCollectionParserTests : TestBase
{
    [TestMethod]
    public void Should_parse_tests_with_name_and_description()
    {
        var sut = new PostmanFolderItemParser();
        var folderCollection = new PostmanFolderItem("path", new List<IPostmanItem>
        {
            CreateTestItem(new Item
            {
                Name = "Test 1",
                Request = new Request
                {
                    Description = "Some test text"
                }
            }),
            CreateTestItem(new Item
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
        var result = sut.Parse(CreateParserArgs(folderCollection));
        result!.Should().NotBeNull();
        result.LocalTestCases.Should().HaveCount(2);

        result.Name.Should().Be("My collection");
        result.Description.Should().Be("Some text");
        result.LocalTestCases[0].Name.Should().Be("Test 1");
        result.LocalTestCases[0].Description.Should().Be("Some test text");
    }

    [TestMethod]
    public void Should_parse_metadata_from_doc_metadata_section()
    {
        var sut = new PostmanFolderItemParser();
        var folderCollection = new PostmanFolderItem("path", new List<IPostmanItem>
            {
                CreateTestItem(new Item
                {
                    Name = "Test 1",
                    Request = new Request
                    {
                        Description = @"# Documentation
This is the documentation

## Metadata

- adoTestCase: 1234
- tags:
    - tag1
    - tag2
- links:
    - story: 4321
    - bug:[4455](https://dev.azure.com/specsync-demo/specsync-plugins-demo/_workitems/edit/4455)
- SpecSync-Source-Version:I0474cd17454fc79d55bcc66f3e5d2753,C730ae229103857834a04f63af9269423
"
                    }
                }),
            }, new Collection());
        var result = sut.Parse(CreateParserArgs(folderCollection));
        var testItem = result.LocalTestCases.ElementAtOrDefault(0) as PostmanTestItem;
        testItem.Should().NotBeNull();

        testItem!.Metadata.ContainsKey("adoTestCase").Should().BeTrue();
        testItem.Metadata["adoTestCase"].StringValue.Should().Be("1234");
        testItem.Metadata.ContainsKey("tags").Should().BeTrue();
        var tags = testItem.Metadata["tags"] as MetadataListValue;
        tags.Should().NotBeNull();
        tags!.Items.Should().HaveCount(2);
        tags.Items[0].Should().BeOfType<MetadataStringValue>().Which.Value.Should().Be("tag1");
        tags.Items[1].Should().BeOfType<MetadataStringValue>().Which.Value.Should().Be("tag2");
        var links = testItem.Metadata["links"] as MetadataListValue;
        links.Should().NotBeNull();
        links!.Items.Should().HaveCount(2);
        links.Items[0].Should().BeOfType<MetadataProperty>().Which.Key.Should().Be("story");
        links.Items[0].Should().BeOfType<MetadataProperty>().Which.Value.StringValue.Should().Be("4321");
        links.Items[1].Should().BeOfType<MetadataProperty>().Which.Key.Should().Be("bug");
        links.Items[1].Should().BeOfType<MetadataProperty>().Which.Value.StringValue.Should().Be("4455");
    }

    [TestMethod]
    public void Should_parse_tags_from_doc_metadata_section()
    {
        var sut = new PostmanFolderItemParser();
        var folderCollection = new PostmanFolderItem("path", new List<IPostmanItem>
            {
                CreateTestItem(new Item
                {
                    Name = "Test 1",
                    Request = new Request
                    {
                        Description = @"# Documentation
This is the documentation

## Metadata

- tc: 1234
- tags:
    - tag1
    - tag2
- SpecSync-Source-Version:I0474cd17454fc79d55bcc66f3e5d2753,C730ae229103857834a04f63af9269423
"
                    }
                }),
            }, new Collection());
        var result = sut.Parse(CreateParserArgs(folderCollection));
        var testItem = result.LocalTestCases.ElementAtOrDefault(0) as PostmanTestItem;
        testItem.Should().NotBeNull();

        testItem!.Tags.Should().NotBeNull();
        testItem.Tags.Should().HaveCount(2);
        testItem.Tags[0].Name.Should().Be("tag1");
        testItem.Tags[0].Should().BeOfType<CodeFileLocalTestCaseTag>().Which.SourceSpan.StartLine.Should().Be(7);
        testItem.Tags[1].Name.Should().Be("tag2");
        testItem.Tags[1].Should().BeOfType<CodeFileLocalTestCaseTag>().Which.SourceSpan.StartLine.Should().Be(8);
    }

    [TestMethod]
    public void Should_parse_tags_from_links_metadata_section()
    {
        var sut = new PostmanFolderItemParser();
        var folderCollection = new PostmanFolderItem("path", new List<IPostmanItem>
            {
                CreateTestItem(new Item
                {
                    Name = "Test 1",
                    Request = new Request
                    {
                        Description = @"# Documentation
This is the documentation

## Metadata

- tc: 1234
- tags:
    - tag1
- links:
    - story: 4321
    - bug:[4455](https://dev.azure.com/specsync-demo/specsync-plugins-demo/_workitems/edit/4455)
- SpecSync-Source-Version:I0474cd17454fc79d55bcc66f3e5d2753,C730ae229103857834a04f63af9269423
"
                    }
                }),
            }, new Collection());
        var result = sut.Parse(CreateParserArgs(folderCollection));
        var testItem = result.LocalTestCases.ElementAtOrDefault(0) as PostmanTestItem;
        testItem.Should().NotBeNull();

        testItem!.Tags.Should().NotBeNull();
        testItem.Tags.Should().HaveCount(3);
        testItem.Tags[0].Name.Should().Be("tag1");
        testItem.Tags[1].Name.Should().Be("story:4321");
        testItem.Tags[1].Should().BeOfType<CodeFileLocalTestCaseTag>().Which.SourceSpan.Should().NotBeNull();
        testItem.Tags[1].Should().BeOfType<CodeFileLocalTestCaseTag>().Which.SourceSpan.StartLine.Should().Be(9);
        testItem.Tags[2].Name.Should().Be("bug:4455");
        testItem.Tags[2].Should().BeOfType<CodeFileLocalTestCaseTag>().Which.SourceSpan.StartLine.Should().Be(10);
    }

    [TestMethod]
    public void Should_parse_TestCaseLink_from_links_metadata_section()
    {
        var sut = new PostmanFolderItemParser();
        var folderCollection = new PostmanFolderItem("path", new List<IPostmanItem>
            {
                CreateTestItem(new Item
                {
                    Name = "Test 1",
                    Request = new Request
                    {
                        Description = @"# Documentation
This is the documentation

## Metadata

- tc: 1234
"
                    }
                }),
            }, new Collection());
        var result = sut.Parse(CreateParserArgs(folderCollection));
        var testItem = result.LocalTestCases.ElementAtOrDefault(0) as PostmanTestItem;
        testItem.Should().NotBeNull();

        testItem!.TestCaseLink.Should().NotBeNull();
        testItem.TestCaseLink.TestCaseId.ToString().Should().Be("1234");
    }

    [TestMethod]
    public void Should_parse_TestCaseLink_from_links_metadata_section_with_BranchTag_customization()
    {
        var sut = new PostmanFolderItemParser();
        var folderCollection = new PostmanFolderItem("path", new List<IPostmanItem>
            {
                CreateTestItem(new Item
                {
                    Name = "Test 1",
                    Request = new Request
                    {
                        Description = @"# Documentation
This is the documentation

## Metadata

- tc: 1234
- branchTc: 2345
"
                    }
                }),
            }, new Collection());
        Configuration.Customizations.BranchTag.Enabled = true;
        Configuration.Customizations.BranchTag.Prefix = "branchTc";
        var result = sut.Parse(CreateParserArgs(folderCollection));
        var testItem = result.LocalTestCases.ElementAtOrDefault(0) as PostmanTestItem;
        testItem.Should().NotBeNull();

        testItem!.TestCaseLink.Should().NotBeNull();
        testItem.TestCaseLink.TestCaseId.ToString().Should().Be("2345");
    }
}