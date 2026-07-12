namespace Domu.Api.Interface.Mobile.Bootstrap;

public sealed class AppBootstrapOptions
{
    public string? LatestVersion { get; init; }
    public string? MinimumSupportedVersion { get; init; }
    public string? AndroidStoreUrl { get; init; }
    public string? IosStoreUrl { get; init; }
}