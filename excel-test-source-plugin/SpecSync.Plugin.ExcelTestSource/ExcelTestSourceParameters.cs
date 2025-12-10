namespace SpecSync.Plugin.ExcelTestSource;

/// <summary>
/// Specifies settings about the Excel file to be processed.
/// </summary>
public class ExcelTestSourceParameters
{
    /// <summary>
    /// The column name contains the Test Case ID.
    /// </summary>
    public string TestCaseIdColumnName { get; set; } = "ID";

    /// <summary>
    /// Whether the ID cell is updated with the prefixed ID ("tc:1234)".
    /// </summary>
    public bool WriteIdWithPrefix { get; set; } = false;

    /// <summary>
    /// The column name contains the Test Case title.
    /// </summary>
    public string TitleColumnName { get; set; } = "Title";

    /// <summary>
    /// The column name contains the Test Case test steps.
    /// </summary>
    public string TestStepColumnName { get; set; } = "Test Step";

    /// <summary>
    /// The column name contains the Test Case test step actions.
    /// </summary>
    public string TestStepActionColumnName { get; set; } = "Step Action";

    /// <summary>
    /// The column name contains the Test Case test step expected results.
    /// </summary>
    public string TestStepExpectedColumnName { get; set; } = "Step Expected";

    /// <summary>
    /// The column name contains the Test Case tags.
    /// </summary>
    public string TagsColumnName { get; set; } = "Tags";

    /// <summary>
    /// The column name contains the Test Case description.
    /// </summary>
    public string DescriptionColumnName { get; set; } = "Description";

    /// <summary>
    /// The column name contains the Test Case automation status.
    /// </summary>
    public string AutomationStatusColumnName { get; set; } = "Automation Status";

    /// <summary>
    /// The column name contains the Test Case automated test name.
    /// </summary>
    public string AutomatedTestNameColumnName { get; set; } = "Automated Test Name";


    public List<FieldUpdateColumnParameter> FieldUpdateColumns { get; set; } = new();

    public void Verify()
    {
    }
}