using Domu.Api.Features.ShoppingLists.Domain.ShoppingLists;

namespace Domu.Tests.Features.ShoppingLists.Domain.ShoppingLists;

public sealed class ShoppingListTests
{
    [Fact]
    public void Rename_CleansName()
    {
        var shoppingList = CreateShoppingList("Groceries");
        var updatedAt = DateTimeOffset.UtcNow;

        shoppingList.Rename("  Weekly   Groceries  ", updatedAt);

        Assert.Equal("Weekly Groceries", shoppingList.Name);
        Assert.Equal(updatedAt, shoppingList.UpdatedAt);
    }

    [Fact]
    public void Rename_WithEmptyName_ThrowsListSpecificError()
    {
        var shoppingList = CreateShoppingList("Groceries");

        var action = () => shoppingList.Rename("  ", DateTimeOffset.UtcNow);

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("Shopping list name cannot be empty", exception.Message);
    }

    private static ShoppingList CreateShoppingList(string name)
    {
        var now = DateTimeOffset.UtcNow;
        return new ShoppingList(
            Guid.NewGuid(),
            Guid.NewGuid(),
            name,
            Guid.NewGuid(),
            now,
            now);
    }
}
