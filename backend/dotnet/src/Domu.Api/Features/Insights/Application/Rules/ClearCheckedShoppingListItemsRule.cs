using Domu.Api.Features.Activities.Application;
using Domu.Api.Features.Insights.Application.Contracts;

namespace Domu.Api.Features.Insights.Application.Rules;

/// <summary>
///     Suggests clearing checked shopping-list items when enough completed entries have accumulated.
/// </summary>
/// <remarks>
///     Purpose: keep active shopping lists focused on remaining work.
///     Produces: <c>shopping_list.clear_checked</c> insights with a <c>shopping_list.clear_checked_items</c> action.
///     Trigger: at least five <c>shopping_list_item.checked</c> activities after the latest clear activity for the list.
///     Dedupe: one suggestion per shopping list using <c>shopping_list.clear_checked:list:{shoppingListId}</c>.
/// </remarks>
public sealed class ClearCheckedShoppingListItemsRule : IInsightRule
{
    public string Key => "shopping-list-clear-checked";

    public Task<IReadOnlyList<HouseholdInsightCandidate>> EvaluateAsync(
        InsightContext context,
        CancellationToken cancellationToken)
    {
        var clearedAtByList = context.Activities
            .Where(householdActivity => householdActivity.Action == HouseholdActivityActions.ShoppingListCheckedItemsCleared)
            .Where(householdActivity => householdActivity.TargetId is not null)
            .GroupBy(householdActivity => householdActivity.TargetId!.Value)
            .ToDictionary(
                group => group.Key,
                group => group.Max(householdActivity => householdActivity.OccurredAt));

        var checkedByList = context.Activities
            .Where(householdActivity => householdActivity.Action == HouseholdActivityActions.ShoppingListItemChecked)
            .Select(householdActivity => (Activity: householdActivity,
                ShoppingListId: new ActivityMetadataReader(householdActivity).GetGuid("shoppingListId")))
            .Where(entry => entry.ShoppingListId is not null)
            .Where(entry => !clearedAtByList.TryGetValue(entry.ShoppingListId!.Value, out var clearedAt)
                            || entry.Activity.OccurredAt > clearedAt)
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