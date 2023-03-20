using Moq;
using SpecSync.Synchronization;
using SpecSync.Tracing;

namespace SpecSync.Plugin.PostmanTestSource.Tests;

public abstract class TestBase
{
    protected Mock<ISpecSyncTracer> TracerStub = new();
    protected Mock<ISynchronizationContext> SynchronizationContextStub = new();

    protected TestBase()
    {
        SynchronizationContextStub.SetupGet(c => c.Tracer).Returns(TracerStub.Object);
    }
}