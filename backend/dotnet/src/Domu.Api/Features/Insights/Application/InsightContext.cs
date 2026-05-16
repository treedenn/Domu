using Domu.Api.Features.Events.Domain;

namespace Domu.Api.Features.Insights.Application;

public sealed record InsightContext(
    Guid HouseholdId,
    Guid UserId,
    DateTimeOffset Now,
    IReadOnlyList<UserEvent> Events);
