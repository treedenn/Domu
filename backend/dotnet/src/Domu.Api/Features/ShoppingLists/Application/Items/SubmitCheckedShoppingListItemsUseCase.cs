using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.ShoppingLists.Application.Items.Commands;
using Domu.Api.Features.ShoppingLists.Application.Items.Contracts;
using Domu.Api.Features.ShoppingLists.Application.Items.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;
using Domu.Api.Features.Spaces.Application.Items;

namespace Domu.Api.Features.ShoppingLists.Application.Items;

public sealed class SubmitCheckedShoppingListItemsUseCase(
    IShoppingListRepository shoppingListRepository,
    IShoppingListItemRepository shoppingListItemRepository,
    IHouseholdAccessService householdAccessService,
    IStageInventoryBatchesUseCase stageInventoryBatchesUseCase)
{
    public async Task<InventorySubmissionResult> ExecuteAsync(SubmitCheckedShoppingListItemsCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await ShoppingListPermissionPolicy.EnsureCanAccessListAsync(shoppingListRepository, householdAccessService, command.Actor, command.HouseholdId, command.ShoppingListId, cancellationToken);
        if (command.Items.Select(x => x.ShoppingListItemId).Distinct().Count() != command.Items.Count)
            throw new ArgumentException("A shopping list item may be submitted only once per request.", nameof(command));
        foreach (var row in command.Items)
            Validate(row);

        var outcomes = new List<InventorySubmissionOutcome>();
        foreach (var row in command.Items)
        {
            var line = await ShoppingListPermissionPolicy.GetListItemAsync(shoppingListItemRepository, command.ShoppingListId, row.ShoppingListItemId, cancellationToken);
            if (!line.Checked) { outcomes.Add(new(row.ShoppingListItemId, null, "not_checked")); continue; }
            if (line.SubmittedToInventoryAt is not null) { outcomes.Add(new(row.ShoppingListItemId, null, "already_submitted")); continue; }
            if (line.ItemId is null) { outcomes.Add(new(row.ShoppingListItemId, null, "missing_source_item")); continue; }

            var entryId = await stageInventoryBatchesUseCase.StageAsync(command.HouseholdId, line.ItemId.Value,
                new(line.Count, row.AmountPerUnit, row.Unit, row.State, row.AcquisitionDate, row.ExpirationDate), cancellationToken);
            if (entryId is null) { outcomes.Add(new(row.ShoppingListItemId, null, "stale_source_item")); continue; }

            line.MarkSubmittedToInventory(entryId.Value, DateTimeOffset.UtcNow);
            await shoppingListItemRepository.UpdateAsync(line, cancellationToken);
            await shoppingListItemRepository.SaveChangesAsync(cancellationToken);
            outcomes.Add(new(row.ShoppingListItemId, entryId, null));
        }
        return new InventorySubmissionResult(outcomes);
    }

    private static void Validate(SubmitCheckedShoppingListItem row)
    {
        if (row.ShoppingListItemId == Guid.Empty || row.AmountPerUnit.HasValue != row.Unit.HasValue || row.AmountPerUnit < 0 || (row.Unit.HasValue && (!Enum.IsDefined(row.Unit.Value) || row.Unit == global::Domu.Api.Features.Spaces.Domain.Items.ItemUnit.Unspecified)) || (row.State.HasValue && !Enum.IsDefined(row.State.Value)) || row.AcquisitionDate > row.ExpirationDate)
            throw new ArgumentException("Submitted inventory batch values are invalid.");
    }
}
