using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Insights.Application.Contracts;

namespace Domu.Api.Features.Insights.Application.Rules;

/// <summary>
/// Suggests clearing checked shopping-list items when enough completed entries have accumulated.
/// </summary>
/// <remarks>
/// Purpose: keep active shopping lists focused on remaining work.
/// Produces: <c>shopping_list.clear_checked</c> insights with a <c>shopping_list.clear_checked_items</c> action.
/// Trigger: at least five <c>shopping_list_item.checked</c> events after the latest clear event for the list.
/// Dedupe: one suggestion per shopping list using <c>shopping_list.clear_checked:list:{shoppingListId}</c>.
/// </remarks>
public sealed class ClearCheckedShoppingListItemsRule : IInsightRule
{
    public string Key => "shopping-list-clear-checked";

    public Task<IReadOnlyList<HouseholdInsightCandidate>> EvaluateAsync(
        InsightContext context,
        CancellationToken cancellationToken)
    {
        var clearedAtByList = context.Events
            .Where(userEvent => userEvent.Action == HouseholdEventActions.ShoppingListCheckedItemsCleared)
            .Where(userEvent => userEvent.TargetId is not null)
            .GroupBy(userEvent => userEvent.TargetId!.Value)
            .ToDictionary(
                group => group.Key,
                group => group.Max(userEvent => userEvent.OccurredAt));

        var checkedByList = context.Events
            .Where(userEvent => userEvent.Action == HouseholdEventActions.ShoppingListItemChecked)
            .Select(userEvent => (Event: userEvent, ShoppingListId: new EventMetadataReader(userEvent).GetGuid("shoppingListId")))
            .Where(entry => entry.ShoppingListId is not null)
            .Where(entry => !clearedAtByList.TryGetValue(entry.ShoppingListId!.Value, out var clearedAt)
                            || entry.Event.OccurredAt > clearedAt)
            .GroupBy(entry => entry.ShoppingListId!.Value)
            .Where(group => group.Count() >= 5)
            .Select(group =>
            {
                var score = Math.Min(1, 0.35 + group.Count() * 0.1);
                var insight = new HouseholdInsight(
                    $"shopping-list-clear-checked:{group.Key}",
                    InsightTypes.ShoppingListClearChecked,
                    "Clean up checked items",
                    $"{group.Count()} items have been checked off since the list was last cleared.",
                    score,
                    50,
                    Key,
                    InsightTargetTypes.ShoppingList,
                    group.Key,
                    new InsightAction(
                        InsightActionTypes.ClearCheckedShoppingListItems,
                        InsightTargetTypes.ShoppingList,
                        group.Key,
                        new Dictionary<string, object?>()),
                    new Dictionary<string, object?> { ["checkedCount"] = group.Count() });

                return new HouseholdInsightCandidate($"shopping_list.clear_checked:list:{group.Key}", insight);
            })
            .ToArray();

        return Task.FromResult<IReadOnlyList<HouseholdInsightCandidate>>(checkedByList);
    }
}
