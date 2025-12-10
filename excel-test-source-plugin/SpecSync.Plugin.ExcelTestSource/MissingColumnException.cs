using SpecSync.Utils;

namespace SpecSync.Plugin.ExcelTestSource;
public class MissingColumnException(string columnName, string worksheetName)
    : SpecSyncException($"Unable to find column '{columnName}' on worksheet {worksheetName}")
{
    public string ColumnName { get; } = columnName;
    public string WorksheetName { get; } = worksheetName;
}
