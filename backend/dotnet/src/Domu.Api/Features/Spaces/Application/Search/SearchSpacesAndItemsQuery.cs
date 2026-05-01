namespace Domu.Api.Features.Spaces.Application.Search;

public sealed record SearchSpacesAndItemsQuery(
    Guid HouseholdId,
    string? Text,
    int? ExpiringWithinDays,
    int Limit);
