using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Events.Domain;
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
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            [
                Event(householdId, UserEventActions.ShoppingListItemCreated, Guid.NewGuid(), Metadata(
                    ("shoppingListId", shoppingListId),
                    ("name", " Milk "))),
                Event(householdId, UserEventActions.ShoppingListItemCreated, Guid.NewGuid(), Metadata(
                    ("shoppingListId", shoppingListId),
                    ("name", "milk"))),
                Event(householdId, UserEventActions.ShoppingListItemCreated, Guid.NewGuid(), Metadata(
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
        var events = new List<UserEvent>
        {
            Event(householdId, UserEventActions.ShoppingListCheckedItemsCleared, shoppingListId, "{}", now.AddMinutes(-10))
        };
        events.AddRange(Enumerable.Range(0, 5).Select(index =>
            Event(
                householdId,
                UserEventActions.ShoppingListItemChecked,
                Guid.NewGuid(),
                Metadata(("shoppingListId", shoppingListId)),
                now.AddMinutes(index))));
        events.Add(Event(
            householdId,
            UserEventActions.ShoppingListItemChecked,
            Guid.NewGuid(),
            Metadata(("shoppingListId", shoppingListId)),
            now.AddMinutes(-20)));

        var context = new InsightContext(householdId, Guid.NewGuid(), now, events);

        var result = await new ClearCheckedShoppingListItemsRule().EvaluateAsync(context, CancellationToken.None);

        var candidate = Assert.Single(result);
        Assert.Equal($"shopping_list.clear_checked:list:{shoppingListId}", candidate.DedupeKey);
        Assert.Equal(5, candidate.Insight.Metadata["checkedCount"]);
    }

    private static UserEvent Event(
        Guid householdId,
        string action,
        Guid? targetId,
        string metadataJson,
        DateTimeOffset? occurredAt = null)
    {
        return new UserEvent(
            Guid.NewGuid(),
            occurredAt ?? DateTimeOffset.UtcNow,
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
        return System.Text.Json.JsonSerializer.Serialize(
            values.ToDictionary(value => value.Key, value => value.Value),
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
    }
}
