using AwesomeAssertions;
using SpecSync.Plugin.JestTestSource.Jest;
using SpecSync.PublishTestResults;

namespace SpecSync.PluginDependency.TypeScriptSource.Tests.Jest;

[TestClass]
public class JestJsonResultLoaderTests : ResultLoaderTestsBase
{
    private JestJsonResultLoader CreateSut() => new();

    [TestMethod]
    public void Should_process_jest_json_format_with_json_extension()
    {
        var sut = CreateSut();

        var canProcess = sut.CanProcess(CreateArgs("c:\\temp\\jest-results.json", JestJsonResultLoader.JestJsonResultFormat));

        canProcess.Should().BeTrue();
    }

    [TestMethod]
    public void Should_not_process_non_json_extension()
    {
        var sut = CreateSut();

        var canProcess = sut.CanProcess(CreateArgs("c:\\temp\\jest-results.txt", JestJsonResultLoader.JestJsonResultFormat));

        canProcess.Should().BeFalse();
    }

    [TestMethod]
    public void Should_load_assertion_results()
    {
        const string jsonContent = """
                                   {
                                     "testResults": [
                                       {
                                         "name": "/repo/src/sum.test.ts",
                                         "assertionResults": [
                                           {
                                             "ancestorTitles": ["sum utility"],
                                             "duration": 2,
                                             "failureMessages": [],
                                             "fullName": "sum utility adds two numbers",
                                             "status": "passed",
                                             "title": "adds two numbers"
                                           },
                                           {
                                             "ancestorTitles": ["sum utility"],
                                             "duration": 3,
                                             "failureMessages": ["Expected true", "Received false"],
                                             "fullName": "sum utility failing case",
                                             "status": "failed",
                                             "title": "failing case"
                                           }
                                         ]
                                       }
                                     ]
                                   }
                                   """;
        PrepareResultFile(jsonContent);

        var sut = CreateSut();
        var result = sut.LoadTestResult(CreateArgs(ResultFileName, JestJsonResultLoader.JestJsonResultFormat));

        result.TestFrameworkIdentifier.Should().Be(JestJsonResultLoader.JestJsonFrameworkIdentifier);
        result.TestResults.Should().HaveCount(2);

        result.TestResults[0].Outcome.Should().Be(TestOutcome.Passed);
        result.TestResults[0].ClassName.Should().Be("sum utility");
        result.TestResults[0].TestName.Should().Be("adds two numbers");
        result.TestResults[0].Duration.Should().Be(TimeSpan.FromMilliseconds(2));
        result.TestResults[0].GetProperty(JestJsonResultLoader.TestResultNameProperty, "").Should().Be("/repo/src/sum.test.ts");

        result.TestResults[1].Outcome.Should().Be(TestOutcome.Failed);
        result.TestResults[1].ErrorMessage.Should().Be("Expected true" + Environment.NewLine + "Received false");
        result.TestResults[1].GetProperty(JestJsonResultLoader.TestResultNameProperty, "").Should().Be("/repo/src/sum.test.ts");
    }

    [TestMethod]
    public void Should_load_tagged_test_results()
    {
        const string jsonContent = """
                                   {
                                     "testResults": [
                                       {
                                         "name": "/repo/src/sum.test.ts",
                                         "assertionResults": [
                                           {
                                             "ancestorTitles": ["calculations", "sum utility [@tag1]"],
                                             "fullName": "calculations sum utility [@tag1] adds two numbers [@tag2 @tag3]",
                                             "status": "passed",
                                             "title": "adds two numbers [@tag2 @tag3]"
                                           }
                                         ]
                                       }
                                     ]
                                   }
                                   """;
        PrepareResultFile(jsonContent);

        var sut = CreateSut();
        var result = sut.LoadTestResult(CreateArgs(ResultFileName, JestJsonResultLoader.JestJsonResultFormat));

        result.TestFrameworkIdentifier.Should().Be(JestJsonResultLoader.JestJsonFrameworkIdentifier);
        result.TestResults.Should().HaveCount(1);

        result.TestResults[0].MethodName.Should().Be("adds two numbers [@tag2 @tag3]");
        result.TestResults[0].ClassName.Should().Be("calculations/sum utility [@tag1]");
        result.TestResults[0].TestName.Should().Be("adds two numbers");
        result.TestResults[0].Name.Should().Be("calculations sum utility adds two numbers");
        result.TestResults[0].Categories.Should().BeEquivalentTo("tag1", "tag2", "tag3");
    }

    [TestMethod]
    [DataRow("pending", TestOutcome.Pending)]
    [DataRow("todo", TestOutcome.Pending)]
    [DataRow("skipped", TestOutcome.NotExecuted)]
    [DataRow("unknown-status", TestOutcome.Unknown)]
    public void Should_map_non_passing_statuses(string status, TestOutcome expectedOutcome)
    {
        var jsonContent = """
                          {
                            "testResults": [
                              {
                                "name": "/repo/src/sum.test.ts",
                                "assertionResults": [
                                  {
                                    "ancestorTitles": [],
                                    "duration": 1,
                                    "failureMessages": [],
                                    "status": "{status}",
                                    "title": "sample"
                                  }
                                ]
                              }
                            ]
                          }
                          """.Replace("{status}", status);
        PrepareResultFile(jsonContent);

        var sut = CreateSut();
        var result = sut.LoadTestResult(CreateArgs(ResultFileName, JestJsonResultLoader.JestJsonResultFormat));

        var testResult = result.TestResults.Should().ContainSingle().Subject;
        testResult.Outcome.Should().Be(expectedOutcome);
    }

    [TestMethod]
    public void Should_strip_ansi_color_codes_from_failure_messages()
    {
        const string jsonContent = """
                                   {
                                     "testResults": [
                                       {
                                         "name": "/repo/src/sum.test.ts",
                                         "assertionResults": [
                                           {
                                             "ancestorTitles": ["isEven utility"],
                                             "failureMessages": ["Error: \u001b[2mexpect(\u001b[22m\u001b[31mreceived\u001b[39m).\u001b[22mtoBe(\u001b[32mexpected\u001b[39m)"],
                                             "status": "failed",
                                             "title": "returns false for odd numbers"
                                           }
                                         ]
                                       }
                                     ]
                                   }
                                   """;
        PrepareResultFile(jsonContent);

        var sut = CreateSut();
        var result = sut.LoadTestResult(CreateArgs(ResultFileName, JestJsonResultLoader.JestJsonResultFormat));

        var testResult = result.TestResults.Should().ContainSingle().Subject;
        testResult.ErrorMessage.Should().Be("Error: expect(received).toBe(expected)");
    }
}
