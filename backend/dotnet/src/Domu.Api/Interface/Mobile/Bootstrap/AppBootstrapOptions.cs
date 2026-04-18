namespace Domu.Api.Interface.Mobile.Server;

public sealed class AppBootstrapOptions
{
    public string? LatestVersion { get; init; }
    public string? MinimumSupportedVersion { get; init; }
    public string? AndroidStoreUrl { get; init; }
    public string? IosStoreUrl { get; init; }
}
