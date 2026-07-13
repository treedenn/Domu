using Domu.Api.Features.ShoppingLists.Domain.Items;

namespace Domu.Tests.Features.ShoppingLists.Domain.Items;

public sealed class ShoppingListItemTests
{
    [Fact]
    public void Rename_CleansAndNormalizesName()
    {
        var item = CreateItem("Milk");
        var updatedAt = DateTimeOffset.UtcNow;

        item.Rename("  Milk   Chocolate  ", updatedAt);

        Assert.Equal("Milk Chocolate", item.Name);
        Assert.Equal("milk chocolate", item.NormalizedName);
        Assert.Equal(updatedAt, item.UpdatedAt);
    }

    [Fact]
    public void Rename_WithEmptyName_ThrowsItemSpecificError()
    {
        var item = CreateItem("Milk");

        var action = () => item.Rename("  ", DateTimeOffset.UtcNow);

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("Shopping list item name cannot be empty", exception.Message);
    }

    [Fact]
    public void ChangeQuantity_ToZero_Throws()
    {
        var item = CreateItem("Milk");

        var action = () => item.ChangeQuantity(0, DateTimeOffset.UtcNow);

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("quantity must be greater than 0", exception.Message);
    }

    [Fact]
    public void ChangeContainer_WithUnsupportedUnit_Throws()
    {
        var item = CreateItem("Milk");

        var action = () => item.ChangeContainer(1, "kg", DateTimeOffset.UtcNow);

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("container unit is invalid", exception.Message);
    }

    [Fact]
    public void Check_SetsCheckedMetadata()
    {
        var item = CreateItem("Milk");
        var memberId = Guid.NewGuid();
        var checkedAt = DateTimeOffset.UtcNow;

        item.Check(memberId, checkedAt);

        Assert.True(item.Checked);
        Assert.Equal(memberId, item.CheckedByMemberId);
        Assert.Equal(checkedAt, item.CheckedAt);
    }

    [Fact]
    public void Uncheck_ClearsCheckedMetadata()
    {
        var item = CreateItem("Milk");
        item.Check(Guid.NewGuid(), DateTimeOffset.UtcNow);

        item.Uncheck(DateTimeOffset.UtcNow);

        Assert.False(item.Checked);
        Assert.Null(item.CheckedByMemberId);
        Assert.Null(item.CheckedAt);
    }

    private static ShoppingListItem CreateItem(string name)
    {
        var now = DateTimeOffset.UtcNow;
        return new ShoppingListItem(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            name,
            Guid.NewGuid(),
            now,
            now,
            1);
    }
}
