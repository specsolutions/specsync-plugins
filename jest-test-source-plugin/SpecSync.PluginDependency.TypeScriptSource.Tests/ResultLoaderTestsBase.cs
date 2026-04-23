using System.Text;
using Moq;
using SpecSync.Configuration;
using SpecSync.IO;
using SpecSync.PublishTestResults.Loaders;
using SpecSync.Synchronization;
using SpecSync.Tracing;
using Path = System.IO.Path;
using StreamWriter = System.IO.StreamWriter;

namespace SpecSync.PluginDependency.TypeScriptSource.Tests;

public abstract class ResultLoaderTestsBase
{
    protected string ResultFileName = null!;

    protected readonly Mock<ISpecSyncTracer> TracerMock = new();

    protected void PrepareResultFile(string fileContent)
    {
        ResultFileName = Path.Combine(Path.GetTempPath(), GetType().Name + "_" + Guid.NewGuid().ToString("N") + ".xml");
        File.WriteAllText(ResultFileName, fileContent, Encoding.UTF8);
    }

    protected Stream PrepareResultStream(string fileContent)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(fileContent);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    protected TestResultLoaderProviderArgs CreateArgs(string path, string resultFormat = TestResultFileFormat.Trx)
    {
        var testResultConfiguration = new TestResultConfiguration
        {
            ResultFormat = resultFormat,
            Sources = [new() { Value = path, BaseFolder = "." }],
        };
        var commandContextMock = new Mock<ICommandContext>();
        commandContextMock.Setup(cc => cc.FileSystem).Returns(FileSystem.Instance);
        commandContextMock.Setup(cc => cc.Tracer).Returns(TracerMock.Object);
        return new TestResultLoaderProviderArgs(commandContextMock.Object, testResultConfiguration, path);
    }
}