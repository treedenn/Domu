using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed record UpdateSpaceCommand(
    DomuActor Actor,
    Guid HouseholdId,
    Guid SpaceId,
    string Name,
    string? Description);
