using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Application.Items;

public sealed record UpdateItemCommand(
    DomuActor Actor,
    Guid HouseholdId,
    Guid SpaceId,
    Guid ItemId,
    string Name,
    string? Category,
    string? Barcode,
    int? DefaultPurchaseCount = null,
    decimal? DefaultPurchaseAmountPerUnit = null,
    ItemUnit? DefaultPurchaseUnit = null);
