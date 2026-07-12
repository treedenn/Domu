using System.Text.Json;
using Domu.Api.Features.Events.Domain;

namespace Domu.Api.Features.Insights.Application;

internal sealed class EventMetadataReader(HouseholdEvent userEvent)
{
    private readonly JsonElement? _root = TryParse(userEvent.MetadataJson);

    public string? GetString(string key)
    {
        if (!TryGetProperty(key, out var property))
            return null;

        return property.ValueKind == JsonValueKind.String ? property.GetString() : property.ToString();
    }

    public Guid? GetGuid(string key)
    {
        var value = GetString(key);
        return Guid.TryParse(value, out var guid) ? guid : null;
    }

    private bool TryGetProperty(string key, out JsonElement property)
    {
        if (_root is { ValueKind: JsonValueKind.Object } root && root.TryGetProperty(key, out property))
            return true;

        property = default;
        return false;
    }

    private static JsonElement? TryParse(string metadataJson)
    {
        try
        {
            using var document = JsonDocument.Parse(metadataJson);
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
