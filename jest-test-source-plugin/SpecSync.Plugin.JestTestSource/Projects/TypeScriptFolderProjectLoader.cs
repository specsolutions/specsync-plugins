using SpecSync.IO;
using SpecSync.Projects;

namespace SpecSync.Plugin.JestTestSource.Projects;

public class TypeScriptFolderProjectLoader : FolderProjectLoaderBase
{
    public override string ServiceDescription => "TypeScript file folder loader";
    public override string FileExtension => ".*";
    public override string SourceFileInputType => "TypeScript";

    protected override string[] GetFiles(string folderFullPath, IFileSystem fileSystem)
    {
        return base.GetFiles(folderFullPath, fileSystem)
            .Where(f => 
                f.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith(".jsx", StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith(".ts", StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith(".tsx", StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }
}