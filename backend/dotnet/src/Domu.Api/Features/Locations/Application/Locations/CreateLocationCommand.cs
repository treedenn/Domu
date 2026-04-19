namespace Domu.Api.Features.Locations.Application.Locations;

public sealed record CreateLocationCommand(
    Guid OwnerId,
    string Name,
    string? Description,
    Guid? ParentId);
