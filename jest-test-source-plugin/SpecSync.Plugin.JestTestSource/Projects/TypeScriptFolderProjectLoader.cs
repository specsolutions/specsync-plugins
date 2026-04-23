using SpecSync.Projects;

namespace SpecSync.Plugin.JestTestSource.Projects;

public class TypeScriptFolderProjectLoader : FolderProjectLoaderBase
{
    public override string ServiceDescription => "TypeScript file folder loader";
    public override string FileExtension => ".ts";
    public override string SourceFileInputType => "TypeScript";
}