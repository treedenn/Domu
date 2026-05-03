namespace Domu.Api.Features.Spaces.Application.Search;

public sealed record SearchSpacesAndItemsQuery(
    Guid UserId,
    Guid HouseholdId,
    string? Text,
    int? ExpiringWithinDays,
    int Limit);
