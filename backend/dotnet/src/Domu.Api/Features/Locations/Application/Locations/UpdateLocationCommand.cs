namespace Domu.Api.Features.Locations.Application.Locations;

public sealed record UpdateLocationCommand(
    Guid LocationId,
    string Name,
    string? Description,
    Guid? ParentId);
