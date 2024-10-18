using System;
using System.Collections.Generic;
using System.Linq;
using SpecSync.Configuration;

namespace SpecSync.Plugin.OnlyPublishTestResults;

public class OnlyPublishTestResultsPluginParameters
{
    /// <summary>
    /// The property name contains the Test Case ID.
    /// </summary>
    public string TestCaseIdPropertyName { get; set; } = "";

    public void Verify()
    {
        if (string.IsNullOrEmpty(TestCaseIdPropertyName))
            throw new SpecSyncConfigurationException("The plugin parameter 'testCaseIdPropertyName' has to be specified. Please specify the test result property that contains the Test Case ID for the result.");
    }

    public static OnlyPublishTestResultsPluginParameters FromPluginParameters(Dictionary<string, object> parameters)
    {
        var result = new OnlyPublishTestResultsPluginParameters();
        foreach (var parameter in parameters)
        {
            var property = result.GetType().GetProperties().FirstOrDefault(p =>
                p.Name.Equals(parameter.Key, StringComparison.InvariantCultureIgnoreCase));
            if (property == null)
                throw new SpecSyncConfigurationException($"Invalid parameter: '{parameter.Key}'");
            property.SetValue(result, parameter.Value);
        }

        result.Verify();

        return result;
    }
}