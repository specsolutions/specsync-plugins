using SpecSync.Parsing;
using SpecSync.Plugin.PostmanTestSource.Projects;
using SpecSync.Utils;

namespace SpecSync.Plugin.PostmanTestSource;

public class PostmanCollectionParser : ILocalTestCaseContainerParser
{
    public string ServiceDescription => "Postman Collection Parser";

    public bool CanProcess(LocalTestCaseContainerParseArgs args)
        => args.SourceFile is PostmanFolderItem;

    public ILocalTestCaseContainer Parse(LocalTestCaseContainerParseArgs args)
    {
        var collection = args.SourceFile as PostmanFolderItem ?? throw new SpecSyncException("The parser can only be used for Postman projects");

        return collection;
    }
}