using SpecSync.Utils;

namespace SpecSync.Plugin.ExcelTestSource;
public class MissingColumnException : SpecSyncException
{
    public string ColumnName { get; }
    public string WorksheetName { get; }

    public MissingColumnException(string columnName, string worksheetName)
        : base($"Unable to find column '{columnName}' on worksheet {worksheetName}")
    {
        ColumnName = columnName;
        WorksheetName = worksheetName;
    }
}
