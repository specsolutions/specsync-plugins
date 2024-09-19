using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using SpecSync.Configuration;

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
    /// The regular expression containing a "value" group to convert the value. 
    /// </summary>
    public string TestCaseIdValueRegex { get; set; } = null;

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


    public List<FieldUpdaterColumnParameter> FieldUpdaterColumnParameters { get; set; } = new();

    public void Verify()
    {
    }

    public static ExcelTestSourceParameters FromPluginParameters(Dictionary<string, object> parameters)
    {
        var result = new ExcelTestSourceParameters();
        foreach (var parameter in parameters)
        {
            if (parameter.Key == "fieldUpdateColumns")
            {
                result.FieldUpdaterColumnParameters = LoadFieldUpdaterColumnParameters(parameter.Value);
            }
            else
            {
                var property = result.GetType().GetProperties().FirstOrDefault(p =>
                    p.Name.Equals(parameter.Key, StringComparison.InvariantCultureIgnoreCase));
                if (property == null)
                    throw new SpecSyncConfigurationException($"Invalid parameter: '{parameter.Key}'");
                property.SetValue(result, parameter.Value);
            }
        }

        return result;
    }

    private static List<FieldUpdaterColumnParameter> LoadFieldUpdaterColumnParameters(object fieldUpdateColumnObj)
    {
        if (fieldUpdateColumnObj == null)
            return new List<FieldUpdaterColumnParameter>();

        if (fieldUpdateColumnObj is not JArray fieldUpdateColumnArray)
            throw new SpecSyncConfigurationException("The 'fieldUpdateColumns' must contain an array.");

        var result = fieldUpdateColumnArray.ToObject<List<FieldUpdaterColumnParameter>>()
                     ?? new List<FieldUpdaterColumnParameter>();

        foreach (var parameter in result)
        {
            if (string.IsNullOrEmpty(parameter.ColumnName))
                throw new SpecSyncConfigurationException("The 'fieldUpdateColumns/columnName' must be specified.");
        }

        return result;
    }

}