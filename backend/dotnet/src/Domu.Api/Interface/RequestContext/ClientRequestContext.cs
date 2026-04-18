namespace Domu.Api.Interface.Mobile.Client;

public sealed class ClientRequestContext
{
    public string RequestId { get; init; } = default!;
    public string? App { get; init; }
    public string? Platform { get; init; }
    public string? VersionRaw { get; init; }
    public Version? Version { get; init; }
    public string? BuildRaw { get; init; }
    public int? Build { get; init; }
}
