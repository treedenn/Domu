using Domu.Api.Features.Auth.Domain;

using Domu.Api.Features.Events.Domain;

namespace Domu.Api.Features.Insights.Application;

public sealed record InsightContext(
    Guid HouseholdId,
    DomuActor Actor,
    DateTimeOffset Now,
    IReadOnlyList<HouseholdEvent> Events);
