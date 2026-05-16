namespace Domu.Api.Features.Insights.Application;

public interface IInsightRule
{
    string Key { get; }

    Task<IReadOnlyList<HouseholdInsightCandidate>> EvaluateAsync(
        InsightContext context,
        CancellationToken cancellationToken);
}
