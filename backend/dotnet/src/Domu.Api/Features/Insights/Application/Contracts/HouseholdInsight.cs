namespace Domu.Api.Features.Insights.Application.Contracts;

public sealed record HouseholdInsight(
    string Id,
    string Type,
    string Title,
    string Body,
    double Score,
    int Priority,
    string CreatedFrom,
    string TargetType,
    Guid? TargetId,
    InsightAction? Action,
    IReadOnlyDictionary<string, object?> Metadata);