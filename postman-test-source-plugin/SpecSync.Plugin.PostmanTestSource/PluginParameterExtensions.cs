using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using SpecSync.Plugins;

namespace SpecSync.Plugin.PostmanTestSource;

public static class PluginParameterExtensions
{
    public static TParameters GetParametersAs<TParameters>(this PluginInitializeArgs args, Dictionary<string, object> defaults = null)
    {
        var resolvedParams = defaults == null
            ? new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase)
            : defaults.ToDictionary(
                e => e.Key, 
                e => args.ResolvePlaceholders(e.Value),
                StringComparer.CurrentCultureIgnoreCase);

        foreach (var parameter in args.Parameters)
        {
            resolvedParams[parameter.Key] = args.ResolvePlaceholders(parameter.Value);
        }

        var paramJObject = JObject.FromObject(resolvedParams);
        return paramJObject.ToObject<TParameters>();
    }

    public static object ResolvePlaceholders(this PluginInitializeArgs args, object value)
    {
        var envRegex = new Regex(@"\{env\:(?<env>[^\}\s]+)\}");
        if (value is string stringValue)
        {
            stringValue = envRegex.Replace(stringValue, m => Environment.GetEnvironmentVariable(m.Groups["env"].Value));
            value = stringValue;
        }

        return value;
    }

    public static object ResolvePlaceholders(this ISyncSettings syncSettings, object value)
    {
        value = ResolvePlaceholders((PluginInitializeArgs)null, value);
        var placeholderRegex = new Regex(@"\{(?<name>[^\}\s]+)\}");
        if (value is string stringValue)
        {
            stringValue = placeholderRegex.Replace(stringValue, m =>
            {
                switch (m.Groups["name"].Value)
                {
                    case "project-url":
                        return syncSettings.TfsProjectUrl;
                    case "project-name":
                        return syncSettings.TeamProjectName;
                    case "server-url":
                        return GetServerUrl(syncSettings.TfsProjectUrl);
                }
                return m.Value;
            });
            value = stringValue;
        }

        return value;
    }

    private static string GetServerUrl(string projectUrl)
    {
        int slashIndex = projectUrl.LastIndexOf('/');
        if (slashIndex >= 0)
            return projectUrl.Substring(0, slashIndex);
        return projectUrl;
    }
}