using System.Text.RegularExpressions;

namespace SpecSync.Plugin.ExcelTestResults;
internal static class CellValueConverter
{
    public static string Convert(string value, string valueRegex = null)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (valueRegex != null)
        {
            var match = Regex.Match(value, valueRegex);
            if (match.Success)
            {
                var valueGroup = match.Groups["value"];
                if (valueGroup.Success)
                    value = valueGroup.Value;
            }
        }

        return value;
    }
}
