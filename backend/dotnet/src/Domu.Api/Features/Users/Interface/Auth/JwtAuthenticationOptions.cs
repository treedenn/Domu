namespace Domu.Api.Features.Users.Interface.Auth;

public sealed class JwtAuthenticationOptions
{
    public const string SectionName = "Authentication:External";

    public string? Authority { get; init; }
    public string? Audience { get; init; }
    public bool RequireHttpsMetadata { get; init; } = true;
    public string SubjectClaimType { get; init; } = "sub";
}
