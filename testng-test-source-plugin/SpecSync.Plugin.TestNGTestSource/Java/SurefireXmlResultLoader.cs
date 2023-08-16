using System;
using System.IO;
using SpecSync.PublishTestResults;
using SpecSync.PublishTestResults.Loaders;
using SpecSync.Tracing;

namespace SpecSync.Plugin.TestNGTestSource.Java;

public class SurefireXmlResultLoader : SpecSync.PublishTestResults.Loaders.JUnitXmlResultLoader
{
    public const string SurefireXml = nameof(SurefireXml);

    protected override string TestFrameworkIdentifier => "executor://java/surefire-xml";

    public override string ServiceDescription => $"{SurefireXml}: Maven Surefire XML result";

    public override bool CanProcess(TestResultLoaderProviderArgs args) => 
        args.TestResultConfiguration.IsFileFormat(SurefireXml) && 
        ".xml".Equals(Path.GetExtension(args.TestResultFilePath), StringComparison.InvariantCultureIgnoreCase);

    //TODO: this override can be removed when target SpecSync 3.5 as it was implemented in 3.4.4.
    protected override TestOutcome GetOutcome(string testCaseStatus, JUnitTestCase testCase, ISpecSyncTracer tracer)
    {
        if (string.IsNullOrEmpty(testCaseStatus))
        {
            if (testCase.Failure != null || testCase.Error != null)
                return TestOutcome.Failed;

            return TestOutcome.Passed;
        }

        return base.GetOutcome(testCaseStatus, testCase, tracer);
    }
}
