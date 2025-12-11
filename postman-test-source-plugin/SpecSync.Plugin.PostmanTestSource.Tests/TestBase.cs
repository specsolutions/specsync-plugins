using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpecSync.Analyzing;
using SpecSync.Configuration;
using SpecSync.Integration.FieldUpdaters;
using SpecSync.Integration.RestApiServices;
using SpecSync.IO;
using SpecSync.Parsing;
using SpecSync.Plugin.PostmanTestSource.Postman;
using SpecSync.Plugin.PostmanTestSource.Postman.Models;
using SpecSync.Plugin.PostmanTestSource.Projects;
using SpecSync.Plugins;
using SpecSync.Projects;
using SpecSync.Synchronization;
using SpecSync.Tracing;
using SpecSync.Utils;
using SpecSync.Utils.Code;
using System.Net;
using Path = System.IO.Path;

namespace SpecSync.Plugin.PostmanTestSource.Tests;

public abstract class TestBase
{
    protected const string CollectionId = "123456";

    protected Mock<ISpecSyncTracer> TracerStub = new();
    protected Mock<ICommandContext> CommandContextStub = new();
    protected Mock<ISpecSyncSettings> SpecSyncSettingsStub = new();
    protected Mock<IArtifactSyncContext> ArtifactSyncContextStub = new();
    protected Mock<ITagServices> TagServicesStub = new();
    protected Mock<IPluginServiceRegistry> PluginServiceRegistryStub = new();
    protected Mock<IValuePlaceholderResolver> ValuePlaceholderResolverStub = new();
    protected SpecSyncConfiguration Configuration = new();
    protected readonly PostmanProject Project;
    protected readonly PostmanMetadataParser PostmanMetadataParser;
    protected readonly PostmanApi PostmanApi;
    protected Mock<IPostmanApiConnection> PostmanApiConnectionStub = new();
    protected object? LastPayload;
    protected PostmanTestSourcePlugin.Parameters Parameters = new()
    {
        CollectionId = CollectionId, 
        MetadataHeading = "Metadata",
        TestCaseLinkTemplate = "https://myserver/myproject/{id}",
        PostmanApiKey = "abc"
    };

    class TestPostmanApiConnectionFactory(Mock<IPostmanApiConnection> postmanApiConnectionStub)
        : PostmanApiConnectionFactory
    {
        public override IPostmanApiConnection Create(ISpecSyncTracer tracer, string postmanApiKey)
        {
            return postmanApiConnectionStub.Object;
        }
    }

    protected TestBase()
    {
        CommandContextStub.SetupGet(c => c.Tracer).Returns(TracerStub.Object);
        CommandContextStub.SetupGet(c => c.Settings).Returns(SpecSyncSettingsStub.Object);
        CommandContextStub.SetupGet(c => c.TagServices).Returns(TagServicesStub.Object);
        CommandContextStub.SetupGet(c => c.FileSystem).Returns(FileSystem.Instance);
        SpecSyncSettingsStub.SetupGet(s => s.Configuration).Returns(Configuration);
        SpecSyncSettingsStub.SetupGet(s => s.BaseFolder).Returns(Directory.GetCurrentDirectory());
        ArtifactSyncContextStub.SetupGet(c => c.CommandContext).Returns(CommandContextStub.Object);
        ValuePlaceholderResolverStub.Setup(r => r.ResolveGenericPlaceholders(It.IsAny<ISpecSyncSettings>(), It.IsAny<object>()))
            .Returns((ISpecSyncSettings _, object v) => v);
        PluginServiceRegistryStub.Setup(r => r.GetBuiltInService<IValuePlaceholderResolver>()).Returns(ValuePlaceholderResolverStub.Object);
        PostmanApiConnectionStub.SetupGet(c => c.Tracer).Returns(TracerStub.Object);
        PostmanApiConnectionStub.Setup(c => c.ExecuteGet<GetCollectionResponse>($"collections/{CollectionId}"))
            .Returns(GetFromFile<GetCollectionResponse>("sample_postman_collection.json"));
        PostmanApiConnectionStub.Setup(c => c.ExecuteGet<JObject>($"collections/{CollectionId}"))
            .Returns(GetFromFile<JObject>("sample_postman_collection.json"));
        PostmanApiConnectionStub.Setup(c => c.ExecuteSend<UpdateCollectionResponse>($"collections/{CollectionId}", It.IsAny<object>(), HttpMethod.Put, false))
            .Returns(new Func<string,object,HttpMethod, bool,RestApiResponse<UpdateCollectionResponse>>((_, data, _, _) =>
            {
                LastPayload = data;
                return new RestApiResponse<UpdateCollectionResponse>(HttpStatusCode.OK, "OK", null,
                    new HttpResponseMessage(HttpStatusCode.OK),
                    new UpdateCollectionResponse
                        { Collection = new UpdateCollectionResponseCollection { Id = CollectionId } });
            }));
            //.Returns(new RestApiResponse<UpdateCollectionResponse> { ResponseData = new UpdateCollectionResponse { Collection = new UpdateCollectionResponseCollection() { Id = CollectionId } } });
        PostmanApiConnectionFactory.Instance = new TestPostmanApiConnectionFactory(PostmanApiConnectionStub);
        PostmanApi = new PostmanApi(PostmanApiConnectionStub.Object);
        PostmanMetadataParser = new PostmanMetadataParser(Parameters);
        Project = new PostmanProject(new List<PostmanFolderItem>(), Path.GetTempPath(), PostmanApi, Parameters);
    }

    private TData GetFromFile<TData>(string fileName)
    {
        var fileContent = File.ReadAllText(fileName);
        return JsonConvert.DeserializeObject<TData>(fileContent)!;
    }

    protected PostmanTestItem CreateTestItem(Item item, PostmanItemMetadata? parentMetadata = null)
    {
        var metadata = PostmanMetadataParser.ParseMetadata(item, CreateLoaderArgs());

        return new PostmanTestItem(item, metadata, parentMetadata != null ? new[]{ parentMetadata} : Array.Empty<PostmanItemMetadata>());
    }
    protected PluginInitializeArgs CreatePluginInitArgs()
    {
        return new PluginInitializeArgs(PluginServiceRegistryStub.Object, new Dictionary<string, object>(), TracerStub.Object, Configuration, "push");
    }

    protected SyncProjectLoaderArgs CreateLoaderArgs()
    {
        return new SyncProjectLoaderArgs(CommandContextStub.Object);
    }

    protected SourceDocumentParserArgs CreateParserArgs(PostmanFolderItem folderCollection)
    {
        return new SourceDocumentParserArgs(Project, folderCollection, CommandContextStub.Object);
    }

    protected PostmanItemMetadata GetEmptyMetadata(string cleanDocumentation = "")
    {
        return new PostmanItemMetadata(
            new EditableCodeFile(new InMemoryWritableTextFile(FileSystem.Instance, cleanDocumentation)),
            "SpecSync",
            null,
            cleanDocumentation);
    }
}