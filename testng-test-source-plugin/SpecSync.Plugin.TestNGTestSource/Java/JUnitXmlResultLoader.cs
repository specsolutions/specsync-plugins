using SpecSync.PublishTestResults.Loaders;

namespace SpecSync.Plugin.TestNGTestSource.Java;

public class JUnitXmlResultLoader : JUnitXmlResultLoaderBase
{
    public const string JUnitXml = nameof(JUnitXml);

    protected override string TestFrameworkIdentifier => "executor://java/junit-xml";

    public override string ServiceDescription => $"{JUnitXml}: JUnit XML result";

    public override bool CanProcess(TestResultLoaderProviderArgs args) => 
        args.TestResultConfiguration.IsResultFormat(JUnitXml) && 
        ".xml".Equals(Path.GetExtension(args.TestResultFilePath), StringComparison.InvariantCultureIgnoreCase);
}
