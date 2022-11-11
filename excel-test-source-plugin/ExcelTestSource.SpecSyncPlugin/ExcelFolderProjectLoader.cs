using SpecSync.Projects;

namespace ExcelTestSource.SpecSyncPlugin;

public class ExcelFolderProjectLoader : FolderProjectLoaderBase
{
    public override string ServiceDescription => "Excel file folder loader";
    public override string FileExtension => ".xlsx";
}