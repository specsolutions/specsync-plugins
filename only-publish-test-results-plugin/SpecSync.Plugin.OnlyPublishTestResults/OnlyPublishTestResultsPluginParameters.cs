using System.Text.RegularExpressions;
using SpecSync.Configuration;

namespace SpecSync.Plugin.OnlyPublishTestResults;

public class OnlyPublishTestResultsPluginParameters
{
    /// <summary>
    /// The property name contains the Test Case ID.
    /// </summary>
    public string TestCaseIdPropertyName { get; set; } = "";
    public string ValueRegex { get; set; } = "";

    public void Verify()
    {
        if (string.IsNullOrEmpty(TestCaseIdPropertyName))
            throw new SpecSyncConfigurationException("The plugin parameter 'testCaseIdPropertyName' has to be specified. Please specify the test result property that contains the Test Case ID for the result.");
        if (!string.IsNullOrEmpty(ValueRegex))
        {
            Regex regex;
            try
            {
                regex = new Regex(ValueRegex);
            }
            catch (Exception ex)
            {
                throw new SpecSyncConfigurationException($"The plugin parameter 'valueRegex' contains an invalid regular expression: {ex.Message}", ex);
            }
            if (!regex.GetGroupNames().Contains("id"))
                throw new SpecSyncConfigurationException("The plugin parameter 'valueRegex' must contain a named group 'id' that contains the Test Case ID value.");
        }
    }
}