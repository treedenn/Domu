using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Insights.Application;

public sealed record GetHouseholdInsightsQuery(Guid HouseholdId, DomuActor Actor);