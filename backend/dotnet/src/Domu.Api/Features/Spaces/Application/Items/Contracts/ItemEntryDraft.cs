using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Application.Items.Contracts;

public sealed record ItemEntryDraft(
    Guid? Id,
    decimal InitialQuantity,
    decimal CurrentQuantity,
    ItemUnit Unit,
    ItemContainerType ContainerType,
    ConsumableState State,
    DateTimeOffset? AcquisitionDate,
    DateTimeOffset? ExpirationDate);