using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Application.Items.Contracts;

public sealed record ItemEntryDraft(
    Guid? Id,
    int Count,
    decimal? OriginalAmountPerUnit,
    decimal? CurrentAmountPerUnit,
    ItemUnit Unit,
    ConsumableState State,
    DateTimeOffset? AcquisitionDate,
    DateTimeOffset? ExpirationDate);
