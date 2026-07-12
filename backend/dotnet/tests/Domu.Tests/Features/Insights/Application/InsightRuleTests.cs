using System.Text.Json;
using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Activities.Application;
using Domu.Api.Features.Activities.Domain;
using Domu.Api.Features.Insights.Application;
using Domu.Api.Features.Insights.Application.Rules;

namespace Domu.Tests.Features.Insights.Application;

public sealed class InsightRuleTests
{
    [Fact]
    public async Task FrequentShoppingListItemRule_SuggestsQuickAddAfterRepeatedCreates()
    {
        var householdId = Guid.NewGuid();
        var shoppingListId = Guid.NewGuid();
        var context = new InsightContext(
            householdId,
            new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel),
            DateTimeOffset.UtcNow,
            [
                Activity(householdId, HouseholdActivityActions.ShoppingListItemCreated, Guid.NewGuid(), Metadata(
                    ("shoppingListId", shoppingListId),
                    ("name", " Milk "))),
                Activity(householdId, HouseholdActivityActions.ShoppingListItemCreated, Guid.NewGuid(), Metadata(
                    ("shoppingListId", shoppingListId),
                    ("name", "milk"))),
                Activity(householdId, HouseholdActivityActions.ShoppingListItemCreated, Guid.NewGuid(), Metadata(
                    ("shoppingListId", shoppingListId),
                    ("name", "MILK")))
            ]);

        var result = await new FrequentShoppingListItemRule().EvaluateAsync(context, CancellationToken.None);

        var candidate = Assert.Single(result);
        Assert.Equal("shopping_list.add_item:name:milk", candidate.DedupeKey);
        Assert.Equal(InsightTypes.ShoppingListFrequentItem, candidate.Insight.Type);
        Assert.Equal(shoppingListId, candidate.Insight.TargetId);
        Assert.Equal("Milk", candidate.Insight.Metadata["name"]);
        Assert.Equal(3, candidate.Insight.Metadata["count"]);
    }

    [Fact]
    public async Task ClearCheckedShoppingListItemsRule_IgnoresChecksBeforeLastClear()
    {
        var householdId = Guid.NewGuid();
        var shoppingListId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var activities = new List<HouseholdActivity>
        {
            Activity(householdId, HouseholdActivityActions.ShoppingListCheckedItemsCleared, shoppingListId, "{}",
                now.AddMinutes(-10))
        };
        activities.AddRange(Enumerable.Range(0, 5).Select(index =>
            Activity(
                householdId,
                HouseholdActivityActions.ShoppingListItemChecked,
                Guid.NewGuid(),
                Metadata(("shoppingListId", shoppingListId)),
                now.AddMinutes(index))));
        activities.Add(Activity(
            householdId,
            HouseholdActivityActions.ShoppingListItemChecked,
            Guid.NewGuid(),
            Metadata(("shoppingListId", shoppingListId)),
            now.AddMinutes(-20)));

        var context =
            new InsightContext(householdId, new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel), now, activities);

        var result = await new ClearCheckedShoppingListItemsRule().EvaluateAsync(context, CancellationToken.None);

        var candidate = Assert.Single(result);
        Assert.Equal($"shopping_list.clear_checked:list:{shoppingListId}", candidate.DedupeKey);
        Assert.Equal(5, candidate.Insight.Metadata["checkedCount"]);
    }

    private static HouseholdActivity Activity(
        Guid householdId,
        string action,
        Guid? targetId,
        string metadataJson,
        DateTimeOffset? occurredAt = null)
    {
        return new HouseholdActivity(
            Guid.NewGuid(),
            occurredAt ?? DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            action,
            "target",
            targetId,
            householdId,
            metadataJson,
            null,
            null,
            null,
            null,
            null);
    }

    private static string Metadata(params (string Key, object? Value)[] values)
    {
        return JsonSerializer.Serialize(
            values.ToDictionary(value => value.Key, value => value.Value),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}
