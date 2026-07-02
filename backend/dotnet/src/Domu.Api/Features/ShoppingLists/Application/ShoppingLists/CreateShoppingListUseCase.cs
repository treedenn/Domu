using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Commands;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Contracts;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;
using Domu.Api.Features.ShoppingLists.Domain.ShoppingLists;

namespace Domu.Api.Features.ShoppingLists.Application.ShoppingLists;

public sealed class CreateShoppingListUseCase(
    IShoppingListRepository shoppingListRepository,
    IHouseholdAccessService householdAccessService,
    IUserEventRecorder? userEventRecorder = null)
{
    private readonly IUserEventRecorder _userEventRecorder = userEventRecorder ?? NoOpUserEventRecorder.Instance;

    public async Task<ShoppingListView> ExecuteAsync(CreateShoppingListCommand command, CancellationToken cancellationToken)
    {
        await householdAccessService.EnsureCanAccessHouseholdAsync(command.HouseholdId, command.UserId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var shoppingList = new ShoppingList(
            Guid.CreateVersion7(), command.HouseholdId, command.Name, command.UserId, now, now);

        await shoppingListRepository.AddAsync(shoppingList, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.UserId, UserEventActions.ShoppingListCreated, UserEventTargetTypes.ShoppingList,
            shoppingList.Id, command.HouseholdId, EventMetadata.From(("name", shoppingList.Name)), cancellationToken);
        await shoppingListRepository.SaveChangesAsync(cancellationToken);

        return ShoppingListView.FromDomain(shoppingList);
    }
}
