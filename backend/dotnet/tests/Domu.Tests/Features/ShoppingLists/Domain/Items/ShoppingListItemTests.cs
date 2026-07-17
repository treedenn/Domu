using Domu.Api.Features.ShoppingLists.Domain.Items;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Tests.Features.ShoppingLists.Domain.Items;

public sealed class ShoppingListItemTests
{
    [Fact]
    public void NewItem_DefaultsToCountOne()
    {
        Assert.Equal(1, CreateItem("Milk").Count);
    }

    [Fact]
    public void SetPlannedBatch_RejectsNonPositiveCount()
    {
        Assert.Throws<ArgumentException>(() => CreateItem("Milk").SetPlannedBatch(0, null, null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void SetPlannedBatch_RequiresAmountAndUnitTogether()
    {
        Assert.Throws<ArgumentException>(() => CreateItem("Milk").SetPlannedBatch(1, 1, null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void SetPlannedBatch_StoresOptionalDetail()
    {
        var item = CreateItem("Milk");
        item.SetPlannedBatch(2, 1, ItemUnit.Liter, DateTimeOffset.UtcNow);
        Assert.Equal(2, item.Count);
        Assert.Equal(1, item.PlannedAmountPerUnit);
        Assert.Equal(ItemUnit.Liter, item.PlannedUnit);
    }

    private static ShoppingListItem CreateItem(string name)
    {
        var now = DateTimeOffset.UtcNow;
        return new ShoppingListItem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), name, Guid.NewGuid(), now, now, 1);
    }
}
