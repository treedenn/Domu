namespace Domu.Api.Features.Insights.Application;

public sealed record GetHouseholdInsightsQuery(Guid HouseholdId, Guid UserId);
