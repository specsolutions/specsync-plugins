using SpecSync.Configuration;

namespace SpecSync.Plugin.GenericTestResultMatcher;

public class PluginParameters
{
    public string? Name { get; set; }
    public string? ClassName { get; set; }
    public string? MethodName { get; set; }
    public string? StdOut { get; set; }

    public Dictionary<string, string> TestResultProperties { get; set; } = new();

    public void Verify()
    {
        if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(ClassName) && string.IsNullOrEmpty(MethodName) && string.IsNullOrEmpty(StdOut) && !TestResultProperties.Any())
            throw new SpecSyncConfigurationException("At least one of the plugin parameters 'name', 'className', 'methodName' or 'stdOut' has to be specified, or a 'testResultProperties' value must be set.");
    }
}