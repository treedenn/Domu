using Domu.Api.Features.Activities.Application;
using Domu.Api.Features.Spaces.Application.Items.Contracts;
using Domu.Api.Features.Spaces.Application.Items.Ports;
using Domu.Api.Features.Spaces.Application.Spaces;

namespace Domu.Api.Features.Spaces.Application.Items;

public sealed class ReplaceItemEntriesUseCase(
    IItemRepository itemRepository,
    ISpaceAccessService spaceAccessService,
    IHouseholdActivityRecorder? householdActivityRecorder = null)
    : IReplaceItemEntriesUseCase
{
    private readonly IHouseholdActivityRecorder _householdActivityRecorder =
        householdActivityRecorder ?? NoOpHouseholdActivityRecorder.Instance;

    public async Task<ItemView> ExecuteAsync(ReplaceItemEntriesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await spaceAccessService.EnsureCanAccessSpaceAsync(
            command.HouseholdId,
            command.SpaceId,
            command.Actor,
            cancellationToken);

        var item = await itemRepository.GetByIdAsync(command.ItemId, cancellationToken)
                   ?? throw new KeyNotFoundException($"Item '{command.ItemId}' was not found.");
        if (item.SpaceId != command.SpaceId)
            throw new KeyNotFoundException($"Item '{command.ItemId}' was not found.");

        ItemEntryWriter.ReplaceEntries(item, command.Entries);

        await itemRepository.UpdateAsync(item, cancellationToken);
        await _householdActivityRecorder.RecordAsync(
            command.Actor,
            HouseholdActivityActions.ItemEntriesReplaced,
            HouseholdActivityTargetTypes.Item,
            item.Id,
            command.HouseholdId,
            ActivityMetadata.From(("spaceId", command.SpaceId), ("entryCount", item.Entries.Count)),
            cancellationToken);
        await itemRepository.SaveChangesAsync(cancellationToken);

        return ItemView.FromDomain(item);
    }
}