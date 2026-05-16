using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Insights.Application.Contracts;

namespace Domu.Api.Features.Insights.Application.Rules;

/// <summary>
/// Suggests restocking an item after repeated shopping-list check-offs.
/// </summary>
/// <remarks>
/// Purpose: infer repeat consumables from completed shopping activity.
/// Produces: <c>shopping_list.restock_candidate</c> insights with a <c>shopping_list.add_item</c> action.
/// Trigger: at least two recent checked shopping-list items that resolve to the same linked item or normalized name.
/// Dedupe: uses <c>shopping_list.add_item:item:{itemId}</c> when linked, otherwise
/// <c>shopping_list.add_item:name:{normalizedName}</c>.
/// </remarks>
public sealed class RestockCandidateRule : IInsightRule
{
    public string Key => "shopping-list-restock-candidate";

    public Task<IReadOnlyList<HouseholdInsightCandidate>> EvaluateAsync(
        InsightContext context,
        CancellationToken cancellationToken)
    {
        var itemFacts = context.Events
            .Where(userEvent =>
                userEvent.Action is UserEventActions.ShoppingListItemCreated
                    or UserEventActions.ShoppingListItemUpdated)
            .Select(userEvent => (Event: userEvent, Metadata: new EventMetadataReader(userEvent)))
            .Select(entry => new ShoppingListItemFact(
                entry.Event.TargetId,
                entry.Metadata.GetGuid("shoppingListId"),
                entry.Metadata.GetString("name"),
                entry.Metadata.GetGuid("itemId"),
                entry.Event.OccurredAt))
            .Where(fact => fact.ShoppingListItemId is not null)
            .GroupBy(fact => fact.ShoppingListItemId!.Value)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(fact => fact.OccurredAt).First());

        var checkedItems = context.Events
            .Where(userEvent => userEvent.Action == UserEventActions.ShoppingListItemChecked)
            .Select(userEvent => itemFacts.TryGetValue(userEvent.TargetId ?? Guid.Empty, out var fact)
                ? fact
                : null)
            .Where(fact => fact is { Name: not null, ShoppingListId: not null })
            .Cast<ShoppingListItemFact>()
            .ToArray();

        var candidates = checkedItems
            .GroupBy(fact => fact.LinkedItemId?.ToString("N") ?? InsightText.NormalizeName(fact.Name!), StringComparer.Ordinal)
            .Where(group => group.Count() >= 2)
            .Select(group =>
            {
                var latest = group.First();
                var score = Math.Min(1, 0.5 + group.Count() * 0.15);
                var insight = new HouseholdInsight(
                    $"shopping-list-restock:{group.Key}",
                    InsightTypes.ShoppingListRestockCandidate,
                    $"Restock {latest.Name}",
                    $"{latest.Name} has been checked off {group.Count()} times recently.",
                    score,
                    60,
                    Key,
                    latest.LinkedItemId is null ? InsightTargetTypes.ShoppingList : InsightTargetTypes.Item,
                    latest.LinkedItemId ?? latest.ShoppingListId,
                    new InsightAction(
                        InsightActionTypes.AddShoppingListItem,
                        InsightTargetTypes.ShoppingList,
                        latest.ShoppingListId,
                        new Dictionary<string, object?>
                        {
                            ["name"] = latest.Name,
                            ["itemId"] = latest.LinkedItemId
                        }),
                    new Dictionary<string, object?>
                    {
                        ["name"] = latest.Name,
                        ["count"] = group.Count(),
                        ["itemId"] = latest.LinkedItemId,
                        ["shoppingListId"] = latest.ShoppingListId
                    });

                return new HouseholdInsightCandidate(
                    latest.LinkedItemId is null
                        ? $"shopping_list.add_item:name:{InsightText.NormalizeName(latest.Name!)}"
                        : $"shopping_list.add_item:item:{latest.LinkedItemId}",
                    insight);
            })
            .ToArray();

        return Task.FromResult<IReadOnlyList<HouseholdInsightCandidate>>(candidates);
    }

    private sealed record ShoppingListItemFact(
        Guid? ShoppingListItemId,
        Guid? ShoppingListId,
        string? Name,
        Guid? LinkedItemId,
        DateTimeOffset OccurredAt);
}
