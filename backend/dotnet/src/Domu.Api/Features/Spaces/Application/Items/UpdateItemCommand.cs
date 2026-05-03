namespace Domu.Api.Features.Spaces.Application.Items;

public sealed record UpdateItemCommand(
    Guid UserId,
    Guid HouseholdId,
    Guid SpaceId,
    Guid ItemId,
    string Name,
    string? Category,
    string? Barcode);
