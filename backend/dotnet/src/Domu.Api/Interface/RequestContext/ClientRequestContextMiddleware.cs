namespace Domu.Api.Interface.RequestContext;

public sealed class ClientRequestContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IClientRequestContextAccessor clientRequestContextAccessor)
    {
        var versionRaw = HeaderValue(context, ClientHeaderNames.Version);
        var buildRaw = HeaderValue(context, ClientHeaderNames.Build);

        clientRequestContextAccessor.Current = new ClientRequestContext
        {
            RequestId = HeaderValue(context, ClientHeaderNames.RequestId) ?? context.TraceIdentifier,
            App = HeaderValue(context, ClientHeaderNames.App),
            Platform = HeaderValue(context, ClientHeaderNames.Platform),
            VersionRaw = versionRaw,
            Version = Version.TryParse(versionRaw, out var version) ? version : null,
            BuildRaw = buildRaw,
            Build = int.TryParse(buildRaw, out var build) ? build : null
        };

        await next(context);
    }

    private static string? HeaderValue(HttpContext context, string name)
    {
        var value = context.Request.Headers[name].FirstOrDefault();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
