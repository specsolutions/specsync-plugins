using System;
using System.Collections.Generic;
using System.Linq;
using SpecSync.Configuration;

namespace SpecSync.Plugin.GenericTestResultMatcher;

public class PluginParameters
{
    public string Name { get; set; }
    public string ClassName { get; set; }
    public string MethodName { get; set; }

    public void Verify()
    {
        if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(ClassName) && string.IsNullOrEmpty(MethodName))
            throw new SpecSyncConfigurationException("At least one of the plugin parameters 'name', 'className' or 'methodName' has to be specified.");
    }

    public static PluginParameters FromPluginParameters(Dictionary<string, object> parameters)
    {
        var result = new PluginParameters();
        foreach (var parameter in parameters)
        {
            var property = result.GetType().GetProperties().FirstOrDefault(p => p.Name.Equals(parameter.Key, StringComparison.InvariantCultureIgnoreCase));
            if (property == null)
                throw new SpecSyncConfigurationException($"Invalid parameter: '{parameter.Key}'");
            property.SetValue(result, parameter.Value);
        }

        result.Verify();

        return result;
    }
}