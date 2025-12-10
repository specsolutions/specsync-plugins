using SpecSync.PublishTestResults.Loaders;

namespace SpecSync.Plugin.TestNGTestSource.Java;

public class SurefireXmlResultLoader : JUnitXmlResultLoaderBase
{
    public const string SurefireXml = nameof(SurefireXml);

    protected override string TestFrameworkIdentifier => "executor://java/surefire-xml";

    public override string ServiceDescription => $"{SurefireXml}: Maven Surefire XML result";

    public override bool CanProcess(TestResultLoaderProviderArgs args) => 
        args.TestResultConfiguration.IsResultFormat(SurefireXml) && 
        ".xml".Equals(Path.GetExtension(args.TestResultFilePath), StringComparison.InvariantCultureIgnoreCase);
}
