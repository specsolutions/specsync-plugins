using SpecSync.PublishTestResults.Loaders;
using SpecSync.PublishTestResults.Matchers;
using SpecSync.PublishTestResults;
using SpecSync.Tracing;
using System.IO;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using SpecSync.Utils;
using System.Xml.Serialization;

namespace SpecSync.Plugin.PostmanTestSource;

public class NewmanJUnitXmlResultLoader : JUnitXmlTestCaseAsStepResultLoader
{
    public const string TestResultFileFormat = "NewmanJUnitXml";

    protected override string TestFrameworkIdentifier => CucumberJsJUnitXmlResultMatcher.TestFrameworkIdentifier;
    public override string ServiceDescription => $"{TestResultFileFormat}: Postman Newman JUnit XML result";

    public override bool CanProcess(TestResultLoaderProviderArgs args)
        => args.TestResultConfiguration.IsFileFormat(TestResultFileFormat) &&
           ".xml".Equals(Path.GetExtension(args.TestResultFilePath), StringComparison.InvariantCultureIgnoreCase);

    protected override TestOutcome GetOutcome(string testCaseStatus, JUnitTestCase testCase, ISpecSyncTracer tracer)
    {
        if (string.IsNullOrEmpty(testCaseStatus))
        {
            return testCase.Error != null ? TestOutcome.Failed : TestOutcome.Passed;
        }

        return base.GetOutcome(testCaseStatus, testCase, tracer);
    }

    protected override void ProcessTestSuite(JUnitTestSuite testSuite, LocalTestRun result, TestResultLoaderProviderArgs args)
    {
        base.ProcessTestSuite(testSuite, result, args);
        var testDefinition = result.TestDefinitions.LastOrDefault();
        if (testDefinition != null)
        {
            var requestResult = new TestRunTestStepResult
            {
                Outcome = TestOutcome.Passed
            };
            var testResult = testDefinition.Results[0];
            if (!testResult.HasStepResults && testSuite is ExtendedJUnitTestSuite extendedTestSuite && 
                (extendedTestSuite.Errors > 0 || extendedTestSuite.SystemErr != null))
            {
                testResult.Outcome = TestOutcome.Failed;
                var errorMessage = GetErrorFromSystemErr(extendedTestSuite.SystemErr);
                testResult.ErrorMessage = errorMessage;
                testResult.ErrorStackTrace = extendedTestSuite.SystemErr;
                requestResult.Outcome = TestOutcome.Failed;
                requestResult.ErrorMessage = errorMessage;
            }

            testResult.StepResults.Insert(0, requestResult);
        }
    }

    private string GetErrorFromSystemErr(string systemErr)
    {
        if (systemErr == null)
            return null;
        var match = Regex.Match(systemErr, @"Error:\s*(?<message>.*)$", RegexOptions.Multiline);
        if (match.Success)
            return match.Groups["message"].Value.Trim();
        return systemErr;
    }

    public class ExtendedJUnitTestSuite : JUnitTestSuite
    {
        [XmlAttribute("errors")]
        public int Errors { get; set; }

        [XmlElement("system-err")]
        public string SystemErr { get; set; }
    }

    protected virtual JUnitTestSuites DeserializeFromTestSuites(string xmlContent)
    {
        var xmlAttributeOverrides = new XmlAttributeOverrides();
        xmlAttributeOverrides.Add(typeof(JUnitTestSuites), nameof(JUnitTestSuites.TestSuites), new XmlAttributes
        {
            XmlElements = { new XmlElementAttribute("testsuite", typeof(ExtendedJUnitTestSuite)) }
        });
        var serializer = new XmlSerializer(typeof(JUnitTestSuites), xmlAttributeOverrides);
        var reader = new StringReader(xmlContent);
        return (JUnitTestSuites)serializer.Deserialize(reader);
    }

    #region Allow overriding deseralization

    public override LocalTestRun LoadTestResult(TestResultLoaderProviderArgs args)
    {
        var xmlContent = File.ReadAllText(args.TestResultFilePath).Trim();
        JUnitTestSuites testSuites;
        try
        {
            testSuites = DeserializeFromTestSuites(xmlContent);
        }
        catch (Exception ex)
        {
            throw new SpecSyncException($"Unable to parse test result file: {ex.Message}", ex);
        }

        var result = new LocalTestRun
        {
            Name = testSuites.Name,
            TestFrameworkIdentifier = TestFrameworkIdentifier
        };

        foreach (var testSuite in testSuites.TestSuites)
        {
            ProcessTestSuite(testSuite, result, args);
        }

        return result;
    }

    #endregion
}