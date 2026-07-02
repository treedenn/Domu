namespace Domu.Api.Features.ShoppingLists.Application.Items.Commands;

public sealed record SetShoppingListItemCheckedStateCommand(
    Guid UserId,
    Guid HouseholdId,
    Guid ShoppingListId,
    Guid ItemId,
    bool IsChecked);
