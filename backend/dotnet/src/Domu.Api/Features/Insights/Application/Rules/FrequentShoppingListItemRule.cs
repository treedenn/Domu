using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Insights.Application.Contracts;

namespace Domu.Api.Features.Insights.Application.Rules;

/// <summary>
///     Suggests a quick-add shopping-list action when the same item name has been added repeatedly.
/// </summary>
/// <remarks>
///     Purpose: make recurring manual shopping-list entries easier to add again.
///     Produces: <c>shopping_list.frequent_item</c> insights with a <c>shopping_list.add_item</c> action.
///     Trigger: at least three recent <c>shopping_list_item.created</c> events with the same normalized name.
///     Dedupe: shares <c>shopping_list.add_item:name:{normalizedName}</c> with other add-item suggestions.
/// </remarks>
public sealed class FrequentShoppingListItemRule : IInsightRule
{
    public string Key => "shopping-list-frequent-item";

    public Task<IReadOnlyList<HouseholdInsightCandidate>> EvaluateAsync(
        InsightContext context,
        CancellationToken cancellationToken)
    {
        var candidates = context.Events
            .Where(userEvent => userEvent.Action == HouseholdEventActions.ShoppingListItemCreated)
            .Select(userEvent => (Event: userEvent, Metadata: new EventMetadataReader(userEvent)))
            .Select(entry => new
            {
                Name = entry.Metadata.GetString("name"),
                ShoppingListId = entry.Metadata.GetGuid("shoppingListId"),
                entry.Event.OccurredAt
            })
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Name) && entry.ShoppingListId is not null)
            .GroupBy(entry => InsightText.NormalizeName(entry.Name!), StringComparer.Ordinal)
            .Where(group => group.Count() >= 3)
            .Select(group =>
            {
                var latest = group.OrderByDescending(entry => entry.OccurredAt).First();
                var displayName = InsightText.CleanDisplayName(group.OrderBy(entry => entry.OccurredAt).First().Name!);
                var shoppingListId = latest.ShoppingListId!.Value;
                var score = Math.Min(1, 0.45 + group.Count() * 0.12);
                var insight = new HouseholdInsight(
                    $"shopping-list-frequent-item:{group.Key}",
                    InsightTypes.ShoppingListFrequentItem,
                    $"Add {displayName} faster",
                    $"{displayName} has been added {group.Count()} times recently.",
                    score,
                    40,
                    Key,
                    InsightTargetTypes.ShoppingList,
                    shoppingListId,
                    new InsightAction(
                        InsightActionTypes.AddShoppingListItem,
                        InsightTargetTypes.ShoppingList,
                        shoppingListId,
                        new Dictionary<string, object?> { ["name"] = displayName }),
                    new Dictionary<string, object?>
                    {
                        ["name"] = displayName,
                        ["normalizedName"] = group.Key,
                        ["count"] = group.Count()
                    });

                return new HouseholdInsightCandidate(
                    $"shopping_list.add_item:name:{group.Key}",
                    insight);
            })
            .ToArray();

        return Task.FromResult<IReadOnlyList<HouseholdInsightCandidate>>(candidates);
    }
}