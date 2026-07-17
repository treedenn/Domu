using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.ShoppingLists.Application.Items.Commands;

public sealed record SubmitCheckedShoppingListItemsCommand(
    DomuActor Actor,
    Guid HouseholdId,
    Guid ShoppingListId,
    IReadOnlyCollection<SubmitCheckedShoppingListItem> Items);

public sealed record SubmitCheckedShoppingListItem(
    Guid ShoppingListItemId,
    decimal? AmountPerUnit,
    ItemUnit? Unit,
    ConsumableState? State,
    DateTimeOffset? AcquisitionDate,
    DateTimeOffset? ExpirationDate);
