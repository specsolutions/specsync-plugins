using AwesomeAssertions;
using Moq;
using SpecSync.Parsing;
using SpecSync.Plugin.JestTestSource.Jest;
using SpecSync.Plugin.JestTestSource.TypeScriptCode;
using SpecSync.PublishTestResults;
using SpecSync.PublishTestResults.Matchers;
using SpecSync.Synchronization;

namespace SpecSync.PluginDependency.TypeScriptSource.Tests.Jest;

[TestClass]
public class JestTestResultMatcherTests
{
    private static TestRunnerResultMatcherArgs CreateMatcherArgs()
    {
        var commandContextMock = new Mock<ICommandContext>();
        return new TestRunnerResultMatcherArgs(commandContextMock.Object, new LocalTestRun
        {
            TestFrameworkIdentifier = JestJsonResultLoader.JestJsonFrameworkIdentifier
        });
    }

    [TestMethod]
    public void GetInvocationArguments_should_extract_parameter_from_jest_each_result_title()
    {
        var sut = new JestTestResultMatcher();
        var args = CreateMatcherArgs();

        var methodBlock = new TypeScriptFunctionCallBlock("test.each([1,3,5])", [], false, [], null!, null);
        var dataRows = new[]
        {
            new LocalTestCaseDataRow
            {
                new KeyValuePair<string, string>("value", "1")
            }
        };

        var localTestCaseMock = new Mock<JestTestLocalTestCase>(
            "returns false for odd values like %i",
            "returns false for odd values like %i [@tc:263]",
            new[] { "isEven utility" },
            Enumerable.Empty<ILocalArtifactTag>(),
            null!,
            methodBlock,
            null!,
            dataRows,
            new[] { "value" },
            null!,
            null!)
        {
            CallBase = true
        };

        ILocalTestCase localTestCase = localTestCaseMock.Object;
        var testResult = new LocalTestResult
        {
            MethodName = "returns false for odd values like 1 [@tc:263]"
        };

        var result = sut.GetInvocationArguments(testResult, localTestCase, Mock.Of<ISourceDocument>(), args);

        result.Should().NotBeNull();
        result.Should().ContainKey("value").WhoseValue.Should().Be("1");
    }

    [TestMethod]
    public void GetInvocationArguments_should_extract_multiple_parameters_from_jest_each_result_title()
    {
        var sut = new JestTestResultMatcher();
        var args = CreateMatcherArgs();

        var methodBlock = new TypeScriptFunctionCallBlock("test.each([[1,2,3]])", [], false, [], null!, null);
        var dataRows = new[]
        {
            new LocalTestCaseDataRow
            {
                new KeyValuePair<string, string>("left", "1"),
                new KeyValuePair<string, string>("right", "2"),
                new KeyValuePair<string, string>("expected", "3"),
            }
        };

        var localTestCaseMock = new Mock<JestTestLocalTestCase>(
            "adds %i and %i to equal %i",
            "adds %i and %i to equal %i [@tc:262]",
            new[] { "sum utility" },
            Enumerable.Empty<ILocalArtifactTag>(),
            null!,
            methodBlock,
            null!,
            dataRows,
            new[] { "left", "right", "expected" },
            null!,
            null!)
        {
            CallBase = true
        };

        ILocalTestCase localTestCase = localTestCaseMock.Object;
        var testResult = new LocalTestResult
        {
            MethodName = "adds 1 and 2 to equal 3 [@tc:262]"
        };

        var result = sut.GetInvocationArguments(testResult, localTestCase, Mock.Of<ISourceDocument>(), args);

        result.Should().NotBeNull();
        result.Should().Contain(
            new KeyValuePair<string, string>("left", "1"), 
            new KeyValuePair<string, string>("right", "2"), 
            new KeyValuePair<string, string>("expected", "3"));
    }
}
