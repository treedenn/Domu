using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.Items.Contracts;
using Domu.Api.Features.ShoppingLists.Application.Items.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;

namespace Domu.Api.Features.ShoppingLists.Application.Items;

public sealed class UpdateShoppingListItemUseCase(
    IShoppingListRepository shoppingListRepository,
    IShoppingListItemRepository shoppingListItemRepository,
    IHouseholdAccessService householdAccessService)
    : IUpdateShoppingListItemUseCase
{
    public async Task<ShoppingListItemView> ExecuteAsync(
        UpdateShoppingListItemCommand command,
        CancellationToken cancellationToken)
    {
        await ShoppingListItemUseCaseGuards.EnsureCanAccessListAsync(
            shoppingListRepository,
            householdAccessService,
            command.HouseholdId,
            command.ShoppingListId,
            command.UserId,
            cancellationToken);

        await ShoppingListItemUseCaseGuards.ValidateReferencesAsync(
            shoppingListItemRepository,
            command.HouseholdId,
            command.SpaceId,
            command.ItemIdLink,
            cancellationToken);

        var item = await ShoppingListItemUseCaseGuards.GetListItemAsync(
            shoppingListItemRepository,
            command.ShoppingListId,
            command.ItemId,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (command.Name is not null)
            item.Rename(command.Name, now);
        item.ChangeQuantity(command.Quantity, now);
        item.ChangeContainer(command.ContainerQuantity, command.ContainerUnit, now);
        item.ChangeNote(command.Note, now);
        item.LinkSpace(command.SpaceId, now);
        item.LinkItem(command.ItemIdLink, now);
        if (command.SortOrder.HasValue)
            item.MoveTo(command.SortOrder.Value, now);

        await shoppingListItemRepository.UpdateAsync(item, cancellationToken);
        await shoppingListItemRepository.SaveChangesAsync(cancellationToken);

        return ShoppingListItemView.FromDomain(item);
    }
}
