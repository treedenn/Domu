using System.Text.Json.Serialization;

namespace Domu.Api.Interface.Mobile.Bootstrap;

public sealed record AppBootstrapResponse(
    string? ClientVersion,
    string? LatestVersion,
    string? MinimumSupportedVersion,
    bool UpdateAvailable,
    bool UpdateRequired,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? StoreUrl);
