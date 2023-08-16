using SpecSync.Projects;

namespace SpecSync.Plugin.TestNGTestSource.Projects;

public class JavaFolderProjectLoader : FolderProjectLoaderBase
{
    public override string ServiceDescription => "Java file folder loader";
    public override string FileExtension => ".java";
    public override string SourceFileInputType => "Java";
}