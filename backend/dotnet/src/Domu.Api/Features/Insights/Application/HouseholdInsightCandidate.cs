using Domu.Api.Features.Insights.Application.Contracts;

namespace Domu.Api.Features.Insights.Application;

public sealed record HouseholdInsightCandidate(
    string DedupeKey,
    HouseholdInsight Insight);