﻿using FluentAssertions;
using Moq;
using SpecSync.Analyzing;
using SpecSync.Parsing;
using SpecSync.Plugin.PostmanTestSource.Postman.Models;
using SpecSync.Plugin.PostmanTestSource.Projects;
using SpecSync.Synchronization;

namespace SpecSync.Plugin.PostmanTestSource.Tests;

[TestClass]
public class PostmanTestItemAnalyzerTests : TestBase
{
    private ILocalTestCase GetLocalTestCase(PostmanTestItem testItem)
    {
        var parser = new PostmanFolderItemParser(SyncSettingsStub.Object);
        var folderCollection = new PostmanFolderItem("path", new List<IPostmanItem>
            {
                testItem
            },
            new Collection
            {
                Info = new Info
                {
                    Name = "My collection",
                    Description = "Some text"
                }
            });
        var result = parser.Parse(CreateParserArgs(folderCollection));
        result.Should().NotBeNull();
        result.LocalTestCases.Should().HaveCount(1);
        return result.LocalTestCases[0];
    }

    [TestMethod]
    public void Should_analyze_core_data()
    {
        var sut = new PostmanTestItemAnalyzer();

        var testItem = CreateTestItem(new Item
        {
            Name = "Test 1",
            Request = new Request
            {
                Method = "GET",
                Url = new Url
                {
                    Raw = "https://postman-echo.com/get?foo1=bar1&foo2=bar2"
                },
                Description = @"# Documentation
This is the documentation

## Metadata

- tc: 1234
- tags:
    - tag1
    - tag2
"
            }
        });

        TagServicesStub.Setup(ts => ts.GetTagData(It.IsAny<ITestCaseSyncContext>()))
            .Returns(new[] { "tag1", "tag2" });

        var result = sut.Analyze(new LocalTestCaseAnalyzerArgs(GetLocalTestCase(testItem), TestCaseSyncContextStub.Object));
        result.Should().NotBeNull();
        result.Title.Should().Be("Test 1");
        result.Tags.Should().ContainInOrder("tag1", "tag2");
    }

    [TestMethod]
    public void Should_parse_steps_from_requests_and_test_scripts()
    {
        var sut = new PostmanTestItemAnalyzer();

        var testItem = CreateTestItem(new Item
        {
            Name = "Test 1",
            Request = new Request
            {
                Method = "GET",
                Url = new Url
                {
                    Raw = "https://postman-echo.com/get?foo1=bar1&foo2=bar2"
                },
                Description = @"# Documentation
This is the documentation

## Metadata

- tc: 1234
"
            },
            Events = new []
            {
                new Event
                {
                    Listen = "test",
                    Script = new Script
                    {
                        Type = "text/javascript",
                        Exec = new []
                        {
                            "pm.test(\"response is ok\", function () {",
                            "    pm.response.to.have.status(200);",
                            "});",
                            "",
                            "pm.test(\"response body has json with request queries\", function () {",
                            "    pm.response.to.have.jsonBody('args.foo1', 'bar1')",
                            "        .and.have.jsonBody('args.foo2', 'bar2');",
                            "});"
                        }
                    }
                }
            }
        });

        var result = sut.Analyze(new LocalTestCaseAnalyzerArgs(GetLocalTestCase(testItem), TestCaseSyncContextStub.Object));
        result.Should().NotBeNull();
        result.TestSteps.Should().HaveCount(3);
        result.TestSteps[0].Keyword.Should().Be("GET ");
        result.TestSteps[0].Text.ToString().Should().Be("https://postman-echo.com/get?foo1=bar1&foo2=bar2");
        result.TestSteps[1].Keyword.Should().Be("pm.test ");
        result.TestSteps[1].Text.ToString().Should().Be("response is ok");
        result.TestSteps[2].Keyword.Should().Be("pm.test ");
        result.TestSteps[2].Text.ToString().Should().Be("response body has json with request queries");
    }

    [TestMethod]
    public void Should_parse_steps_from_requests_and_test_scripts_for_folder_item_tests()
    {
        var sut = new PostmanTestItemAnalyzer();

        var testItem = CreateTestItem(new Item
        {
            Name = "Folder Test",
            Description = @"# Documentation
This is the documentation

## Metadata

- tc: 1234
",
            Items = new []
            {
                new Item
                {
                    Name = "Request 1",
                    Request = new Request
                    {
                        Method = "GET",
                        Url = new Url
                        {
                            Raw = "https://postman-echo.com/get?foo1=bar1&foo2=bar2"
                        }
                    },
                    Events = new []
                    {
                        new Event
                        {
                            Listen = "test",
                            Script = new Script
                            {
                                Type = "text/javascript",
                                Exec = new []
                                {
                                    "pm.test(\"response is ok\", function () {",
                                    "    pm.response.to.have.status(200);",
                                    "});"
                                }
                            }
                        }
                    }
                },
                new Item
                {
                    Name = "Request 2",
                    Request = new Request
                    {
                        Method = "POST",
                        Url = new Url
                        {
                            Raw = "https://postman-echo.com/get?foo3=bar1&foo4=bar2"
                        }
                    },
                    Events = new []
                    {
                        new Event
                        {
                            Listen = "test",
                            Script = new Script
                            {
                                Type = "text/javascript",
                                Exec = new []
                                {
                                    "pm.test(\"response body has json with request queries\", function () {",
                                    "    pm.response.to.have.jsonBody('args.foo1', 'bar1')",
                                    "        .and.have.jsonBody('args.foo2', 'bar2');",
                                    "});"
                                }
                            }
                        }
                    }
                },
            }
        }
        );

        var result = sut.Analyze(new LocalTestCaseAnalyzerArgs(GetLocalTestCase(testItem), TestCaseSyncContextStub.Object));
        result.Should().NotBeNull();
        result.TestSteps.Should().HaveCount(4);
        result.TestSteps[0].Keyword.Should().Be("GET ");
        result.TestSteps[0].Text.ToString().Should().Be("https://postman-echo.com/get?foo1=bar1&foo2=bar2");
        result.TestSteps[1].Keyword.Should().Be("pm.test ");
        result.TestSteps[1].Text.ToString().Should().Be("response is ok");
        result.TestSteps[2].Keyword.Should().Be("POST ");
        result.TestSteps[2].Text.ToString().Should().Be("https://postman-echo.com/get?foo3=bar1&foo4=bar2");
        result.TestSteps[3].Keyword.Should().Be("pm.test ");
        result.TestSteps[3].Text.ToString().Should().Be("response body has json with request queries");
    }


    [TestMethod]
    [DataRow("pm.test(\"response is ok\", function () {", "response is ok")]
    [DataRow("pm.test(\"response is ok\"  , function () {", "response is ok")]
    [DataRow("pm.test(\"response is ok\"  ,", "response is ok")]
    [DataRow("pm.test('response is ok', function () {", "response is ok")]
    [DataRow("pm.test(`response is ok`, function () {", "response is ok")]
    [DataRow("pm.test(\"response is ok\" + \"(\"+pm.info.requestName+\")\", function () {", "\"response is ok\" + \"(\"+pm.info.requestName+\")\"")]
    [DataRow("pm.test(pm.info.requestName + \": response is ok\", function () {", "pm.info.requestName + \": response is ok\"")]
    [DataRow("pm.test(`${pm.info.requestName}: response is ok`, function () {", "${pm.info.requestName}: response is ok")]
    public void Should_parse_different_pm_test_usage(string pmTestLine, string expectedText)
    {
        var sut = new PostmanTestItemAnalyzer();

        var testItem = CreateTestItem(new Item
        {
            Name = "Test 1",
            Request = new Request(),
            Events = new[]
            {
                new Event
                {
                    Listen = "test",
                    Script = new Script
                    {
                        Type = "text/javascript",
                        Exec = new []{ pmTestLine, "});" }
                    }
                }
            }
        });

        var result = sut.Analyze(new LocalTestCaseAnalyzerArgs(GetLocalTestCase(testItem), TestCaseSyncContextStub.Object));
        result.Should().NotBeNull();
        result.TestSteps.Should().HaveCount(2);
        result.TestSteps[1].Keyword.Should().Be("pm.test ");
        result.TestSteps[1].Text.ToString().Should().Be(expectedText);
    }

    [TestMethod]
    [DataRow("//pm.test(\"response is ok\", function () {")]
    [DataRow("  // foo pm.test(\"response is ok\", function () {")]
    [DataRow("xpm.test(\"response is ok\", function () {")]
    public void Should_not_parse_commented_pm_test_usage(string pmTestLine)
    {
        var sut = new PostmanTestItemAnalyzer();

        var testItem = CreateTestItem(new Item
        {
            Name = "Test 1",
            Request = new Request(),
            Events = new[]
            {
                new Event
                {
                    Listen = "test",
                    Script = new Script
                    {
                        Type = "text/javascript",
                        Exec = new []{ pmTestLine, "});" }
                    }
                }
            }
        });

        var result = sut.Analyze(new LocalTestCaseAnalyzerArgs(GetLocalTestCase(testItem), TestCaseSyncContextStub.Object));
        result.Should().NotBeNull();
        result.TestSteps.Should().HaveCount(1);
    }
}