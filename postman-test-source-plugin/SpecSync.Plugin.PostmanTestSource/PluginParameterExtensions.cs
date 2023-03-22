using System;
using Newtonsoft.Json.Linq;
using SpecSync.Plugins;

namespace SpecSync.Plugin.PostmanTestSource;

public static class PluginParameterExtensions
{
    public static TParameters GetParametersAs<TParameters>(this PluginInitializeArgs args)
    {
        var paramJObject = JObject.FromObject(args.Parameters);
        return paramJObject.ToObject<TParameters>();
    }
}