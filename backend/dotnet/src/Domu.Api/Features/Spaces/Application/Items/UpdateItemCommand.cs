using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Spaces.Application.Items;

public sealed record UpdateItemCommand(
    DomuActor Actor,
    Guid HouseholdId,
    Guid SpaceId,
    Guid ItemId,
    string Name,
    string? Category,
    string? Barcode);
