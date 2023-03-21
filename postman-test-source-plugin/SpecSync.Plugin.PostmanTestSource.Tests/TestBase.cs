using Moq;
using Newtonsoft.Json;
using SpecSync.Analyzing;
using SpecSync.Configuration;
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
    protected Mock<ISpecSyncTracer> TracerStub = new();
    protected Mock<ISynchronizationContext> SynchronizationContextStub = new();
    protected Mock<ISyncSettings> SyncSettingsStub = new();
    protected Mock<ITestCaseSyncContext> TestCaseSyncContextStub = new();
    protected Mock<ITagServices> TagServicesStub = new();
    protected SpecSyncConfiguration Configuration = new();
    protected readonly Mock<IBddProject> ProjectStub = new();
    protected readonly PostmanMetadataParser _postmanMetadataParser = new();
    protected Mock<IPostmanApiConnection> PostmanApiConnectionStub = new();

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
        PostmanApiConnectionStub.Setup(c => c.ExecuteGet<GetCollectionResponse>(It.IsAny<string>()))
            .Returns(GetFromFile<GetCollectionResponse>("sample_postman_collection.json"));
        PostmanApiConnectionFactory.Instance = new TestPostmanApiConnectionFactory(PostmanApiConnectionStub);
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
        return new LocalTestCaseContainerParseArgs(ProjectStub.Object, folderCollection, SynchronizationContextStub.Object);
    }
}