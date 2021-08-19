using System;
using System.Collections.Generic;
using System.Linq;
using SpecSync.AzureDevOps.Configuration;

namespace ExcelTestResults.SpecSyncPlugin
{
    /// <summary>
    /// Specifies settings about the Excel file to be processed.
    /// </summary>
    public class ExcelResultSpecification
    {
        /// <summary>
        /// The sheet name that contains the test results. Optional, uses the first sheet if not specified.
        /// </summary>
        public string TestResultSheetName { get; set; }
        /// <summary>
        /// The column name that contains the feature name. Optional, should be specified when scenario names are not globally unique and <see cref="TestCaseIdColumnName"/> is not specified.
        /// </summary>
        public string FeatureColumnName { get; set; }
        /// <summary>
        /// The column name that contains the feature file name. Optional, should be specified when scenario names are not globally unique and <see cref="TestCaseIdColumnName"/> is not specified.
        /// </summary>
        public string FeatureFileColumnName { get; set; }
        /// <summary>
        /// The column name contains the scenario name. Optional, must be specified when <see cref="TestCaseIdColumnName"/> is not specified.
        /// </summary>
        public string ScenarioColumnName { get; set; }
        /// <summary>
        /// The column name contains the outcome (Passed, Failed). Mandatory.
        /// </summary>
        public string OutcomeColumnName { get; set; }
        /// <summary>
        /// The column name contains the Test Case ID. Optional, must be specified when <see cref="ScenarioColumnName"/> is not specified.
        /// </summary>
        public string TestCaseIdColumnName { get; set; }
        /// <summary>
        /// The column name contains the name (displayed in Azure DevOps). Optional, the first column is used if not specified.
        /// </summary>
        public string TestNameColumnName { get; set; }
        /// <summary>
        /// The column name contains the error message. Optional, no error message is recoded if not specified.
        /// </summary>
        public string ErrorMessageColumnName { get; set; }

        public bool MatchByTestCaseId => !string.IsNullOrEmpty(TestCaseIdColumnName);
        public bool MatchByScenario => !string.IsNullOrEmpty(ScenarioColumnName);
        public bool MatchByFeature => !string.IsNullOrEmpty(FeatureColumnName);
        public bool MatchByFeatureFile => !string.IsNullOrEmpty(FeatureFileColumnName);

        public void Verify()
        {
            if (string.IsNullOrEmpty(OutcomeColumnName))
                throw new SpecSyncConfigurationException($"The property {nameof(ExcelResultSpecification)}.{nameof(OutcomeColumnName)} must be specified.");

            if (string.IsNullOrEmpty(ScenarioColumnName) && string.IsNullOrEmpty(TestCaseIdColumnName))
                throw new SpecSyncConfigurationException($"Either {nameof(ScenarioColumnName)} or {nameof(TestCaseIdColumnName)} must be specified on {nameof(ExcelResultSpecification)}.");
        }

        public static ExcelResultSpecification FromPluginParameters(Dictionary<string, object> parameters)
        {
            var result = new ExcelResultSpecification();
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
}
