using Moq;
using SpecSync.Configuration;
using SpecSync.Synchronization;
using SpecSync.Tracing;

namespace SpecSync.Plugin.PostmanTestSource.Tests;

public abstract class TestBase
{
    protected Mock<ISpecSyncTracer> TracerStub = new();
    protected Mock<ISynchronizationContext> SynchronizationContextStub = new();
    protected Mock<ISyncSettings> SyncSettingsStub = new();
    protected SpecSyncConfiguration Configuration = new();

    protected TestBase()
    {
        SynchronizationContextStub.SetupGet(c => c.Tracer).Returns(TracerStub.Object);
        SynchronizationContextStub.SetupGet(c => c.Settings).Returns(SyncSettingsStub.Object);
        SyncSettingsStub.SetupGet(s => s.Configuration).Returns(Configuration);
    }
}