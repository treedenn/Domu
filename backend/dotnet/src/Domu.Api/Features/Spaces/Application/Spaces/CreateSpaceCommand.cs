using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed record CreateSpaceCommand(
    DomuActor Actor,
    Guid HouseholdId,
    string Name,
    string? Description,
    Guid? ParentId);
