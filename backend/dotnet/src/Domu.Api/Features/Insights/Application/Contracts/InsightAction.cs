namespace Domu.Api.Features.Insights.Application.Contracts;

public sealed record InsightAction(
    string Type,
    string TargetType,
    Guid? TargetId,
    IReadOnlyDictionary<string, object?> Metadata);
