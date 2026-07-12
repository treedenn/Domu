namespace Domu.Api.Features.Activities.Application;

public sealed class ActivityMetadata : Dictionary<string, object?>
{
    public static ActivityMetadata Empty()
    {
        return [];
    }

    public static ActivityMetadata From(params (string Key, object? Value)[] values)
    {
        var metadata = new ActivityMetadata();
        foreach (var (key, value) in values)
            if (!string.IsNullOrWhiteSpace(key) && value is not null)
                metadata[key] = value;

        return metadata;
    }
}