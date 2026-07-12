using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.Insights.Application.Contracts;

namespace Domu.Api.Features.Insights.Application;

public sealed class GetHouseholdInsightsUseCase(
    IHouseholdAccessService householdAccessService,
    IUserEventQueryService userEventQueryService,
    IEnumerable<IInsightRule> rules)
    : IGetHouseholdInsightsUseCase
{
    private const int MaxInsights = 8;
    private static readonly TimeSpan Lookback = TimeSpan.FromDays(90);

    public async Task<HouseholdInsightsView> ExecuteAsync(
        GetHouseholdInsightsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        await householdAccessService.EnsureCanAccessHouseholdAsync(
            query.Actor,
            query.HouseholdId,
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var events = await userEventQueryService.GetRecentHouseholdEventsAsync(
            query.HouseholdId,
            now.Subtract(Lookback),
            cancellationToken);
        var context = new InsightContext(query.HouseholdId, query.Actor, now, events);

        var candidates = new List<HouseholdInsightCandidate>();
        foreach (var rule in rules)
            candidates.AddRange(await rule.EvaluateAsync(context, cancellationToken));

        var insights = candidates
            .GroupBy(candidate => candidate.DedupeKey, StringComparer.Ordinal)
            .Select(group => group
                .OrderByDescending(candidate => candidate.Insight.Score)
                .ThenBy(candidate => candidate.Insight.Id, StringComparer.Ordinal)
                .First()
                .Insight)
            .OrderByDescending(insight => insight.Priority)
            .ThenByDescending(insight => insight.Score)
            .ThenBy(insight => insight.Id, StringComparer.Ordinal)
            .Take(MaxInsights)
            .ToArray();

        return new HouseholdInsightsView(insights);
    }
}
