using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.ShoppingLists.Application.Items.Commands;

public sealed record CreateShoppingListItemCommand(
    DomuActor Actor,
    Guid HouseholdId,
    Guid ShoppingListId,
    string Name,
    string? Note,
    Guid? SpaceId,
    Guid? ItemId,
    int Count = 1,
    decimal? AmountPerUnit = null,
    ItemUnit? Unit = null);
