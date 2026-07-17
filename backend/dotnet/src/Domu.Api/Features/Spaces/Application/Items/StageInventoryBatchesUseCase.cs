using Domu.Api.Features.Spaces.Application.Items.Ports;
using Domu.Api.Features.Spaces.Application.Spaces;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Application.Items;

public sealed class StageInventoryBatchesUseCase(IItemRepository itemRepository, ISpaceAccessService spaceAccessService)
    : IStageInventoryBatchesUseCase
{
    public async Task<Guid?> StageAsync(Guid householdId, Guid itemId, InventoryBatchDraft batch, CancellationToken cancellationToken)
    {
        if (batch.Count <= 0 || batch.AmountPerUnit.HasValue != batch.Unit.HasValue || batch.AmountPerUnit < 0 || (batch.Unit.HasValue && (!Enum.IsDefined(batch.Unit.Value) || batch.Unit == ItemUnit.Unspecified)) || (batch.State.HasValue && !Enum.IsDefined(batch.State.Value)))
            throw new ArgumentException("Inventory batch values are invalid.", nameof(batch));
        if (batch.AcquisitionDate > batch.ExpirationDate)
            throw new ArgumentException("Acquisition date cannot be after expiration date.", nameof(batch));

        var item = await itemRepository.GetByIdAsync(itemId, cancellationToken);
        if (item is null)
            return null;
        try { await spaceAccessService.EnsureSpaceBelongsToHouseholdAsync(item.SpaceId, householdId, cancellationToken); }
        catch (KeyNotFoundException) { return null; }

        var entry = new ItemEntry(Guid.CreateVersion7(), item.Id);
        entry.SetBatch(batch.Count, batch.AmountPerUnit, batch.AmountPerUnit);
        entry.SetUnit(batch.Unit ?? ItemUnit.Unspecified);
        entry.ChangeState(batch.State ?? ConsumableState.Unspecified);
        entry.SetDates(batch.AcquisitionDate, batch.ExpirationDate);
        item.AddEntry(entry);
        await itemRepository.UpdateAsync(item, cancellationToken);
        await itemRepository.SaveChangesAsync(cancellationToken);
        return entry.Id;
    }
}
