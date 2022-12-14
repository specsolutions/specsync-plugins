using System.IO;
using System.Linq;
using SpecSync.Projects;

namespace ExcelTestSource.SpecSyncPlugin;

public class ExcelFolderProjectLoader : FolderProjectLoaderBase
{
    public override string ServiceDescription => "Excel file folder loader";
    public override string FileExtension => ".xlsx";
    public override string SourceFileInputType => "Excel";

    protected override string[] GetFiles(string folderFullPath)
    {
        return base.GetFiles(folderFullPath).Where(f => !Path.GetFileName(f).StartsWith("~")).ToArray();
    }
}