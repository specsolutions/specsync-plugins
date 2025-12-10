namespace SpecSync.Plugin.ExcelTestSource;

public class FieldUpdateColumnParameter(string columnName, string fieldName, string? tagNamePrefix = null)
{
    public string ColumnName { get; } = columnName;
    public string? FieldName { get; } = fieldName;
    public string? TagNamePrefix { get; } = tagNamePrefix;

    public string GeneratedTagNamePrefix => $"{ExcelTestSourcePlugin.InternalTagPrefix}{ActualFieldName.Replace(" ", "_")}:";
    public string ActualFieldName => FieldName ?? ColumnName;
}