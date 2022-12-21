using System;
using System.Collections.Generic;
using System.Linq;
using SpecSync.Configuration;

namespace SpecSync.Plugin.ExcelTestResults;

/// <summary>
/// Specifies settings about the Excel file to be processed.
/// </summary>
public class ExcelResultParameters
{
    /// <summary>
    /// The sheet name that contains the test results. Optional, uses the first sheet if not specified.
    /// </summary>
    public string TestResultSheetName { get; set; }

    /// <summary>
    /// The column name that contains the feature name.
    /// </summary>
    public string FeatureColumnName { get; set; } = "Feature";
    /// <summary>
    /// The column name that contains the feature file name.
    /// </summary>
    public string FeatureFileColumnName { get; set; } = "Feature File";

    /// <summary>
    /// The column name contains the scenario name.
    /// </summary>
    public string ScenarioColumnName { get; set; } = "Scenario";

    /// <summary>
    /// The column name contains the outcome (Passed, Failed). Mandatory.
    /// </summary>
    public string OutcomeColumnName { get; set; } = "Outcome";

    /// <summary>
    /// The column name contains the Test Case ID. 
    /// </summary>
    public string TestCaseIdColumnName { get; set; } = "ID";

    /// <summary>
    /// The column name contains the name (displayed in Azure DevOps).
    /// </summary>
    public string TestNameColumnName { get; set; } = "Test Name";

    /// <summary>
    /// The column name contains the error message. Optional, no error message is recoded if not specified.
    /// </summary>
    public string ErrorMessageColumnName { get; set; } = "Error";

    public void Verify()
    {
    }

    public static ExcelResultParameters FromPluginParameters(Dictionary<string, object> parameters)
    {
        var result = new ExcelResultParameters();
        foreach (var parameter in parameters)
        {
            var property = result.GetType().GetProperties().FirstOrDefault(p => p.Name.Equals(parameter.Key, StringComparison.InvariantCultureIgnoreCase));
            if (property == null)
                throw new SpecSyncConfigurationException($"Invalid parameter: '{parameter.Key}'");
            property.SetValue(result, parameter.Value);
        }

        return result;
    }
}