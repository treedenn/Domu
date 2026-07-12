using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Spaces.Application.Search;

public sealed record SearchSpacesAndItemsQuery(
    DomuActor Actor,
    Guid HouseholdId,
    string? Text,
    int? ExpiringWithinDays,
    int Limit);
