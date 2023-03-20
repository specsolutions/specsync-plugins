using FluentAssertions;
using SpecSync.Configuration;
using SpecSync.PublishTestResults;
using SpecSync.PublishTestResults.Loaders;

namespace SpecSync.Plugin.PostmanTestSource.Tests;

[TestClass]
public class NewmanJUnitXmlResultLoaderTests : TestBase
{
    private TestResultLoaderProviderArgs CreateArgs(string? testFilePath = null)
    {
        testFilePath ??= "sample_newman_result.xml";
        return new TestResultLoaderProviderArgs(SynchronizationContextStub.Object, new TestResultConfiguration(), testFilePath);
    }

    [TestMethod]
    public void Should_get_result_with_passing_pm_test_checks()
    {
        var sut = new NewmanJUnitXmlResultLoader();

        var result = sut.LoadTestResult(CreateArgs());

        result.Should().NotBeNull();
        result.TestDefinitions.Should().HaveCountGreaterThan(0);
        var testWithTwoPassingPmTestCheck = result.TestDefinitions[0].Results.First();
        testWithTwoPassingPmTestCheck.Outcome.Should().Be(TestOutcome.Passed);
        testWithTwoPassingPmTestCheck.StepResults.Should().HaveCount(3)
            .And.AllSatisfy(r => r.Outcome.Should().Be(TestOutcome.Passed));
    }

    [TestMethod]
    public void Should_get_result_with_failing_pm_test_checks()
    {
        var sut = new NewmanJUnitXmlResultLoader();

        var result = sut.LoadTestResult(CreateArgs());

        result.Should().NotBeNull();
        result.TestDefinitions.Should().HaveCountGreaterThan(1);
        var testWithTwoFailingPmTestCheck = result.TestDefinitions[1].Results.First();
        testWithTwoFailingPmTestCheck.Outcome.Should().Be(TestOutcome.Failed);
        testWithTwoFailingPmTestCheck.StepResults.Should().HaveCount(3);
        testWithTwoFailingPmTestCheck.StepResults[0].Outcome.Should().Be(TestOutcome.Passed);
        testWithTwoFailingPmTestCheck.StepResults[1].Outcome.Should().Be(TestOutcome.Failed);
        testWithTwoFailingPmTestCheck.StepResults[1].ErrorMessage.Should().Be("expected response to have status code 200 but got 401");
        testWithTwoFailingPmTestCheck.StepResults[1].ErrorStackTrace.Should().NotBeNullOrEmpty();
        testWithTwoFailingPmTestCheck.StepResults[2].Outcome.Should().Be(TestOutcome.Failed);
    }

    [TestMethod]
    public void Should_get_result_with_request_without_pm_test_checks()
    {
        var sut = new NewmanJUnitXmlResultLoader();

        var result = sut.LoadTestResult(CreateArgs());

        result.Should().NotBeNull();
        result.TestDefinitions.Should().HaveCountGreaterThan(4);
        var passingSimpleRequest = result.TestDefinitions[4].Results.First();
        passingSimpleRequest.Outcome.Should().Be(TestOutcome.Passed);
        passingSimpleRequest.StepResults.Should().HaveCount(1);
        passingSimpleRequest.StepResults[0].Outcome.Should().Be(TestOutcome.Passed);
    }

    [TestMethod]
    public void Should_get_result_with_failing_request_without_pm_test_checks()
    {
        var sut = new NewmanJUnitXmlResultLoader();

        var result = sut.LoadTestResult(CreateArgs());

        result.Should().NotBeNull();
        result.TestDefinitions.Should().HaveCountGreaterThan(3);
        var failingSimpleRequest = result.TestDefinitions[3].Results.First();
        failingSimpleRequest.Name.Should().Be("Server Events / GET Server events");
        failingSimpleRequest.Outcome.Should().Be(TestOutcome.Failed);
        failingSimpleRequest.ErrorMessage.Should().Be("Error: getaddrinfo ENOTFOUND postman-echox.com");
        failingSimpleRequest.ErrorStackTrace.Should().NotBeNullOrEmpty();
        failingSimpleRequest.StepResults.Should().HaveCount(1);
        failingSimpleRequest.StepResults[0].Outcome.Should().Be(TestOutcome.Failed);
        failingSimpleRequest.StepResults[0].ErrorMessage.Should().Be("Error: getaddrinfo ENOTFOUND postman-echox.com");
        failingSimpleRequest.StepResults[0].ErrorMessage.Should().NotBeNullOrEmpty();
    }
}