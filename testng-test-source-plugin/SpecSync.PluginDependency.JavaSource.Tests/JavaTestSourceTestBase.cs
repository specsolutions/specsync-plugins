using System.IO;

namespace SpecSync.PluginDependency.JavaSource.Tests;

public abstract class JavaTestSourceTestBase
{
    protected string GetFile(string fileName)
    {
        var filePath = GetFilePath(fileName);
        return File.ReadAllText(filePath);
    }

    protected string GetFilePath(string fileName)
    {
        var projectFolder = GetProjectFolder();
        var filePath = Path.Combine(projectFolder, fileName);
        return filePath;
    }

    protected string GetProjectFolder()
    {
        var testAssemblyFolder = Path.GetDirectoryName(GetType().Assembly.Location);
        var projectFolder = Path.Combine(testAssemblyFolder!, "TestContent", "SampleProject");
        return projectFolder;
    }
}