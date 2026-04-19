using Domu.Api.Features.Locations.Domain.Items;

namespace Domu.Api.Features.Locations.Application.Items.Contracts;

public sealed record ItemEntryDraft(
    Guid? Id,
    int Quantity,
    ConsumableState State,
    DateTimeOffset? AcquisitionDate,
    DateTimeOffset? ExpirationDate);
