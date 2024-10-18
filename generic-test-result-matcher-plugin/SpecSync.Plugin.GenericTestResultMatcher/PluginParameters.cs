using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using SpecSync.Configuration;

namespace SpecSync.Plugin.GenericTestResultMatcher;

public class PluginParameters
{
    public string Name { get; set; }
    public string ClassName { get; set; }
    public string MethodName { get; set; }
    public string StdOut { get; set; }

    public Dictionary<string, string> TestResultProperties { get; set; } = new();

    public void Verify()
    {
        if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(ClassName) && string.IsNullOrEmpty(MethodName) && string.IsNullOrEmpty(StdOut) && !TestResultProperties.Any())
            throw new SpecSyncConfigurationException("At least one of the plugin parameters 'name', 'className', 'methodName' or 'stdOut' has to be specified, or a 'testResultProperties' value must be set.");
    }

    public static PluginParameters FromPluginParameters(Dictionary<string, object> parameters)
    {
        var result = new PluginParameters();
        foreach (var parameter in parameters)
        {
            if (parameter.Key.Equals(nameof(TestResultProperties), StringComparison.InvariantCultureIgnoreCase))
            {
                result.TestResultProperties = LoadStringDictionary(parameter.Value, parameter.Key);
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

        result.Verify();

        return result;
    }

    private static Dictionary<string, string> LoadStringDictionary(object value, string columnName)
    {
        if (value == null)
            return new Dictionary<string, string>();

        if (value is not JObject valueObject || !valueObject.PropertyValues().All(pv => pv is JValue))
            throw new SpecSyncConfigurationException($"The '{columnName}' must contain a JSON object with string values.");

        var objDict = (IDictionary<string, JToken>)valueObject;
        return objDict.ToDictionary(item => item.Key, item => (item.Value as JValue)?.Value?.ToString() ?? "");
    }
}