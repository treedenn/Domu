using System.Text.RegularExpressions;

namespace Domu.Api.Features.ShoppingLists.Domain.Items;

public static partial class ShoppingListText
{
    public static string CleanName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Shopping list item name cannot be empty.", nameof(name));

        return WhitespaceRegex().Replace(name.Trim(), " ");
    }

    public static string NormalizeName(string name)
    {
        return CleanName(name).ToLowerInvariant();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
