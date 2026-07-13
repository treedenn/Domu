using System.Text.RegularExpressions;

namespace Domu.Api.Features.ShoppingLists.Domain;

internal static partial class NameNormalizer
{
    public static string Clean(string name)
    {
        return WhitespaceRegex().Replace(name.Trim(), " ");
    }

    public static string NormalizeForComparison(string name)
    {
        return Clean(name).ToLowerInvariant();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
