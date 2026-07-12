namespace Domu.Api.Features.Events.Application;

public sealed class EventMetadata : Dictionary<string, object?>
{
    public static EventMetadata Empty()
    {
        return [];
    }

    public static EventMetadata From(params (string Key, object? Value)[] values)
    {
        var metadata = new EventMetadata();
        foreach (var (key, value) in values)
            if (!string.IsNullOrWhiteSpace(key) && value is not null)
                metadata[key] = value;

        return metadata;
    }
}