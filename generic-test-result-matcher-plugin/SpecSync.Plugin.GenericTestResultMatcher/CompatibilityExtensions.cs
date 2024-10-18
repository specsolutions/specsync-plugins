using SpecSync.Parsing;

namespace SpecSync.Plugin.GenericTestResultMatcher;
internal static class CompatibilityExtensions
{
    //TODO: replace by ltc.Name with SpecSync v3.5
    public static string GetName(this ILocalTestCase ltc)
    {
        return (string)ltc.GetType().GetProperty("Name")?.GetValue(ltc);
    }
}
