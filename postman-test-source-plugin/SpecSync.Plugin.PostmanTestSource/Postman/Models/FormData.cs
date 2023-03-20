using System.Linq;
using Newtonsoft.Json.Linq;

namespace SpecSync.Plugin.PostmanTestSource.Postman.Models;

public class FormData
{
    private object _src;
    public string Key { get; set; }

    public string Value { get; set; }

    public string ContentType { get; set; }

    public string Type { get; set; }

    /// <summary>
    ///     String list of file paths.
    /// </summary>
    public object Src
    {
        get
        {
            if (_src is JArray a)
                return a.Select(x => x.ToString()).ToArray();
            if (_src is string s)
                return new[] { s };
            return _src;
        }
        set => _src = value;
    }

    public bool Disabled { get; set; }
}