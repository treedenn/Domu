namespace Domu.Api.Features.Locations.Application.Locations.Contracts;

public sealed record LocationChildView(
    Guid Id,
    Guid OwnerId,
    Guid? ParentId,
    string Name,
    string? Description);
