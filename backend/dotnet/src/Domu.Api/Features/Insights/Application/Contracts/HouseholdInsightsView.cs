namespace Domu.Api.Features.Insights.Application.Contracts;

public sealed record HouseholdInsightsView(IReadOnlyList<HouseholdInsight> Insights);