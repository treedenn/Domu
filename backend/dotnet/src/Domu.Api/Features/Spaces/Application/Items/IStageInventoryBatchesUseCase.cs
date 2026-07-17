using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Application.Items;

/// <summary>Stable cross-feature contract for adding a purchased stock batch to an existing item.</summary>
public interface IStageInventoryBatchesUseCase
{
    Task<Guid?> StageAsync(Guid householdId, Guid itemId, InventoryBatchDraft batch, CancellationToken cancellationToken);
}

public sealed record InventoryBatchDraft(
    int Count,
    decimal? AmountPerUnit,
    ItemUnit? Unit,
    ConsumableState? State,
    DateTimeOffset? AcquisitionDate,
    DateTimeOffset? ExpirationDate);
