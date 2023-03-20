using FluentAssertions;
using Moq;
using SpecSync.Configuration;
using SpecSync.Plugin.PostmanTestSource.Projects;
using SpecSync.Projects;
using SpecSync.Synchronization;
using SpecSync.Tracing;

namespace SpecSync.Plugin.PostmanTestSource.Tests;

[TestClass]
public class PostmanCollectionLoaderTests
{

    [TestMethod]
    public void ShouldLoadFolderAndTestNodesFromCollection()
    {
        var sut = new PostmanCollectionLoader();
        var tracerStub = new Mock<ISpecSyncTracer>();
        var synchronizationContextStub = new Mock<ISynchronizationContext>();
        synchronizationContextStub.SetupGet(c => c.Tracer).Returns(tracerStub.Object);

        var project = sut.LoadProject(new BddProjectLoaderArgs(synchronizationContextStub.Object, new LocalConfiguration(), Path.GetTempPath()));
        project.Should().NotBeNull();
        project.LocalTestContainerFiles.Should().HaveCountGreaterThan(0);
        var postmanProject = project as PostmanProject;
        postmanProject.Should().NotBeNull();
        postmanProject!.Collections.Should().Contain(c => c.Tests.Any());
    }
}