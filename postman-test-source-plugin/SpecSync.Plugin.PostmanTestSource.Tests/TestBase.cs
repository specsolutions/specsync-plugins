using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpecSync.Analyzing;
using SpecSync.Configuration;
using SpecSync.Integration.RestApiServices;
using SpecSync.Parsing;
using SpecSync.Plugin.PostmanTestSource.Postman;
using SpecSync.Plugin.PostmanTestSource.Postman.Models;
using SpecSync.Plugin.PostmanTestSource.Projects;
using SpecSync.Projects;
using SpecSync.Synchronization;
using SpecSync.Tracing;

namespace SpecSync.Plugin.PostmanTestSource.Tests;

public abstract class TestBase
{
    protected const string CollectionId = "123456";

    protected Mock<ISpecSyncTracer> TracerStub = new();
    protected Mock<ISynchronizationContext> SynchronizationContextStub = new();
    protected Mock<ISyncSettings> SyncSettingsStub = new();
    protected Mock<ITestCaseSyncContext> TestCaseSyncContextStub = new();
    protected Mock<ITagServices> TagServicesStub = new();
    protected SpecSyncConfiguration Configuration = new();
    protected readonly PostmanProject Project;
    protected readonly PostmanMetadataParser _postmanMetadataParser = new();
    protected Mock<IPostmanApiConnection> PostmanApiConnectionStub = new();
    protected object? LastPayload;

    class TestPostmanApiConnectionFactory : PostmanApiConnectionFactory
    {
        private readonly Mock<IPostmanApiConnection> _postmanApiConnectionStub;
        public TestPostmanApiConnectionFactory(Mock<IPostmanApiConnection> postmanApiConnectionStub)
        {
            _postmanApiConnectionStub = postmanApiConnectionStub;
        }

        public override IPostmanApiConnection Create(ISpecSyncTracer tracer)
        {
            return _postmanApiConnectionStub.Object;
        }
    }

    protected TestBase()
    {
        SynchronizationContextStub.SetupGet(c => c.Tracer).Returns(TracerStub.Object);
        SynchronizationContextStub.SetupGet(c => c.Settings).Returns(SyncSettingsStub.Object);
        SynchronizationContextStub.SetupGet(c => c.TagServices).Returns(TagServicesStub.Object);
        SyncSettingsStub.SetupGet(s => s.Configuration).Returns(Configuration);
        TestCaseSyncContextStub.SetupGet(c => c.SynchronizationContext).Returns(SynchronizationContextStub.Object);
        PostmanApiConnectionStub.Setup(c => c.ExecuteGet<GetCollectionResponse>($"collections/{CollectionId}"))
            .Returns(GetFromFile<GetCollectionResponse>("sample_postman_collection.json"));
        PostmanApiConnectionStub.Setup(c => c.ExecuteGet<JObject>($"collections/{CollectionId}"))
            .Returns(GetFromFile<JObject>("sample_postman_collection.json"));
        PostmanApiConnectionStub.Setup(c => c.ExecutePut<UpdateCollectionResponse>($"collections/{CollectionId}", It.IsAny<object>(), false))
            .Returns(new Func<string,object,bool,RestApiResponse<UpdateCollectionResponse>>((_, data, _) =>
            {
                LastPayload = data;
                return new RestApiResponse<UpdateCollectionResponse>
                {
                    ResponseData = new UpdateCollectionResponse
                        { Collection = new UpdateCollectionResponseCollection { Id = CollectionId } }
                };
            }));
            //.Returns(new RestApiResponse<UpdateCollectionResponse> { ResponseData = new UpdateCollectionResponse { Collection = new UpdateCollectionResponseCollection() { Id = CollectionId } } });
        PostmanApiConnectionFactory.Instance = new TestPostmanApiConnectionFactory(PostmanApiConnectionStub);
        Project = new PostmanProject(new List<PostmanFolderItem>(), Path.GetTempPath(), new PostmanApi(PostmanApiConnectionStub.Object), CollectionId);
    }

    private TData GetFromFile<TData>(string fileName)
    {
        var fileContent = File.ReadAllText(fileName);
        return JsonConvert.DeserializeObject<TData>(fileContent)!;
    }

    protected PostmanTestItem CreateTestItem(Item item)
    {
        var metadata = _postmanMetadataParser.ParseMetadata(item);
        return new PostmanTestItem(item, metadata);
    }


    protected LocalTestCaseContainerParseArgs CreateParserArgs(PostmanFolderItem folderCollection)
    {
        return new LocalTestCaseContainerParseArgs(Project, folderCollection, SynchronizationContextStub.Object);
    }
}