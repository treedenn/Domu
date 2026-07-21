using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Spaces.Application.Expirations;

public sealed record GetHouseholdExpirationsQuery(
    DomuActor Actor,
    Guid HouseholdId,
    DateTimeOffset UpcomingUntilUtc);
