using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Application.Expirations.Contracts;

public sealed record HouseholdExpirationsView(
    DateTimeOffset EvaluatedAtUtc,
    IReadOnlyList<ExpirationBatchView> Expired,
    IReadOnlyList<ExpirationBatchView> Upcoming);

public sealed record ExpirationBatchView(
    Guid EntryId,
    int Count,
    decimal? OriginalAmountPerUnit,
    decimal? CurrentAmountPerUnit,
    ItemUnit Unit,
    ConsumableState State,
    DateTimeOffset? AcquisitionDate,
    DateTimeOffset ExpirationDate,
    Guid ItemId,
    string ItemName,
    Guid SpaceId,
    string SpaceName);
