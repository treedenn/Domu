using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Application.Items.Contracts;

public sealed record ItemEntryDraft(
    Guid? Id,
    decimal OriginalQuantity,
    decimal CurrentQuantity,
    ItemUnit Unit,
    ConsumableState State,
    DateTimeOffset? AcquisitionDate,
    DateTimeOffset? ExpirationDate);
