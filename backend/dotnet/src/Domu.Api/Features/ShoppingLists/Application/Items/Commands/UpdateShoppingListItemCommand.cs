using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.ShoppingLists.Application.Items.Commands;

public sealed record UpdateShoppingListItemCommand(
    DomuActor Actor,
    Guid HouseholdId,
    Guid ShoppingListId,
    Guid ItemId,
    string? Name,
    string? Note,
    Guid? SpaceId,
    Guid? ItemIdLink,
    decimal? SortOrder,
    int Count = 1,
    decimal? AmountPerUnit = null,
    ItemUnit? Unit = null);
