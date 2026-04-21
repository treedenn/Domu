using Domu.Api.Interface.RequestContext;
using Microsoft.Extensions.Options;

namespace Domu.Api.Interface.Mobile.Bootstrap;

public sealed class AppBootstrapService(
    IClientRequestContextAccessor clientRequestContextAccessor,
    IOptions<AppBootstrapOptions> options)
    : IAppBootstrapService
{
    private readonly ClientRequestContext _client = clientRequestContextAccessor.Current;
    private readonly AppBootstrapOptions _options = options.Value;

    public AppBootstrapResponse BuildResponse()
    {
        var latestVersion = TryParseVersion(_options.LatestVersion);
        var minimumSupportedVersion = TryParseVersion(_options.MinimumSupportedVersion);
        var clientVersion = _client.Version;

        var updateRequired = clientVersion is not null &&
                             minimumSupportedVersion is not null &&
                             clientVersion < minimumSupportedVersion;

        var updateAvailable = clientVersion is not null &&
                              latestVersion is not null &&
                              clientVersion < latestVersion;

        return new AppBootstrapResponse(
            _client.VersionRaw,
            _options.LatestVersion,
            _options.MinimumSupportedVersion,
            updateAvailable,
            updateRequired,
            updateAvailable || updateRequired ? ResolveStoreUrl(_client.Platform) : null);
    }

    private string? ResolveStoreUrl(string? platform)
    {
        if (string.Equals(platform, "android", StringComparison.OrdinalIgnoreCase))
        {
            return NullIfWhitespace(_options.AndroidStoreUrl);
        }

        if (string.Equals(platform, "ios", StringComparison.OrdinalIgnoreCase))
        {
            return NullIfWhitespace(_options.IosStoreUrl);
        }

        return null;
    }

    private static Version? TryParseVersion(string? value)
        => Version.TryParse(value, out var version) ? version : null;

    private static string? NullIfWhitespace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value;
}
