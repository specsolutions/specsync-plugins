using SpecSync.Parsing;
using System;
using System.Linq;

namespace SpecSync.Plugin.PostmanTestSource;

/// <summary>
/// Used by SpecSync to recognize keywords in steps coming from Azure DevOps
/// </summary>
public class PostmanKeywordParser : IKeywordParser
{
    public static readonly PostmanKeywordParser Instance = new();
    public bool TryParseStepKeyword(string text, out string keyword, out string remainingText, out string stepPrefix)
    {
        keyword = null;
        remainingText = text;
        stepPrefix = null;

        foreach (var item in new []{ "pm.test", "DELETE", "GET", "HEAD", "OPTIONS", "PATCH", "POST", "PUT", "TRACE", "COPY", "LINK", "UNLINK", "PURGE", "LOCK", "UNLOCK", "PROPFIND", "VIEW" }.Select(e => e.ToString()))
        {
            if (text.StartsWith(item, StringComparison.InvariantCultureIgnoreCase))
            {
                keyword = text.Substring(0, item.Length) + " ";
                remainingText = text.Substring(item.Length).TrimStart();
                return true;
            }
        }

        return true;
    }

    public string GetPrimaryLocalTestCaseKeyword(bool isDataDriven) => null;
    public string[] GetLocalTestCaseKeywords(bool isDataDriven) => Array.Empty<string>();
    public string GetPrimaryLocalTestCaseParametersKeyword() => null;
}