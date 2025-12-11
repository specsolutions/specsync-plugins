using AwesomeAssertions;
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
        return new TestResultLoaderProviderArgs(CommandContextStub.Object, new TestResultConfiguration(), testFilePath);
    }

    [TestMethod]
    public void Should_get_result_with_passing_pm_test_checks()
    {
        var sut = new NewmanJUnitXmlResultLoader();

        var result = sut.LoadTestResult(CreateArgs());

        result.Should().NotBeNull();
        result.TestResults.Should().HaveCountGreaterThan(0);
        var testWithTwoPassingPmTestCheck = result.TestResults[0];
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
        result.TestResults.Should().HaveCountGreaterThan(1);
        var testWithTwoFailingPmTestCheck = result.TestResults[1];
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
        result.TestResults.Should().HaveCountGreaterThan(4);
        var passingSimpleRequest = result.TestResults[4];
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
        result.TestResults.Should().HaveCountGreaterThan(3);
        var failingSimpleRequest = result.TestResults[3];
        failingSimpleRequest.Name.Should().Be("Server Events / GET Server events");
        failingSimpleRequest.Outcome.Should().Be(TestOutcome.Failed);
        failingSimpleRequest.ErrorMessage.Should().Be("Error: getaddrinfo ENOTFOUND postman-echox.com");
        failingSimpleRequest.ErrorStackTrace.Should().NotBeNullOrEmpty();
        failingSimpleRequest.StepResults.Should().HaveCount(1);
        failingSimpleRequest.StepResults[0].Outcome.Should().Be(TestOutcome.Failed);
        failingSimpleRequest.StepResults[0].ErrorMessage.Should().Be("Error: getaddrinfo ENOTFOUND postman-echox.com");
        failingSimpleRequest.StepResults[0].ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public void Should_crate_fake_merged_results_for_folder_tests()
    {
        var sut = new NewmanJUnitXmlResultLoader();

        var result = sut.LoadTestResult(CreateArgs());

        result.Should().NotBeNull();
        var serverEventsResults = result.TestResults.Should().Contain(td => td.Name == "Server Events").Subject;
        serverEventsResults.Outcome.Should().Be(TestOutcome.Failed);
        serverEventsResults.StepResults.Should().HaveCount(2);
        serverEventsResults.StepResults[0].Outcome.Should().Be(TestOutcome.Failed);
        serverEventsResults.StepResults[1].Outcome.Should().Be(TestOutcome.Passed);
        serverEventsResults.ErrorMessage.Should().NotBeNullOrEmpty();
        serverEventsResults.ErrorStackTrace.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public void Should_crate_fake_merged_results_for_folder_tests_up_to_the_root()
    {
        var sut = new NewmanJUnitXmlResultLoader();

        var result = sut.LoadTestResult(CreateArgs());

        result.Should().NotBeNull();
        result.TestResults.Should().ContainSingle(td => td.Name == "Helpers / Date and Time / Current UTC time");
        result.TestResults.Should().ContainSingle(td => td.Name == "Helpers / Date and Time");
        result.TestResults.Should().ContainSingle(td => td.Name == "Helpers");
    }

    [TestMethod]
    public void Should_treat_test_case_result_failed_if_the_failing_step_is_not_the_last()
    {
        var sut = new NewmanJUnitXmlResultLoader();

        var result = sut.LoadTestResult(CreateArgs());

        result.Should().NotBeNull();
        var testResult = result.TestResults.Should().ContainSingle(td => td.Name == "Utilities / Get UTF8 Encoded Response (Fail First)").Subject;
        testResult.Outcome.Should().Be(TestOutcome.Failed);
    }
}