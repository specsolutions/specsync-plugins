namespace ExcelTestSource.SpecSyncPlugin;

public class FieldUpdaterColumnParameter
{
    public string ColumnName { get; }
    public string FieldName { get; }
    public string TagNamePrefix { get; }

    public string GeneratedTagNamePrefix => $"{ExcelTestSourcePlugin.InternalTagPrefix}{ActualFieldName.Replace(" ", "_")}:";
    public string ActualFieldName => FieldName ?? ColumnName;

    public FieldUpdaterColumnParameter(string columnName, string fieldName, string tagNamePrefix = null)
    {
        ColumnName = columnName;
        FieldName = fieldName;
        TagNamePrefix = tagNamePrefix;
    }
}