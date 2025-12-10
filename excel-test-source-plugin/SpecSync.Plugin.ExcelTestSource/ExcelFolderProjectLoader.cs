using SpecSync.IO;
using SpecSync.Projects;
using Path = SpecSync.IO.Path;

namespace SpecSync.Plugin.ExcelTestSource;

public class ExcelFolderProjectLoader : FolderProjectLoaderBase
{
    public override string ServiceDescription => "Excel file folder loader";
    public override string FileExtension => ".xlsx";
    public override string SourceFileInputType => "Excel";

    // The project loader can be forced by setting local/projectType to ExcelSource, but also supports auto-detection.
    public override bool CanProcess(SyncProjectLoaderArgs args) => 
        args.LocalConfiguration.IsType("ExcelSource") || base.CanProcess(args);

    protected override string[] GetFiles(string folderFullPath, IFileSystem fileSystem)
    {
        return base.GetFiles(folderFullPath, fileSystem).Where(f => !Path.GetFileName(f).StartsWith("~")).ToArray();
    }
}