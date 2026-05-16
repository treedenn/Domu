namespace Domu.Api.Features.Insights.Application;

internal static class InsightText
{
    public static string CleanDisplayName(string name)
    {
        return string.Join(
            ' ',
            name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    public static string NormalizeName(string name)
    {
        return CleanDisplayName(name).ToLowerInvariant();
    }
}
