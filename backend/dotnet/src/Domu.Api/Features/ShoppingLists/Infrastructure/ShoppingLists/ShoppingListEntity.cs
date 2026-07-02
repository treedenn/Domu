using Domu.Api.Features.ShoppingLists.Domain.ShoppingLists;
using Domu.Api.Features.ShoppingLists.Infrastructure.Items;

namespace Domu.Api.Features.ShoppingLists.Infrastructure.ShoppingLists;

public sealed class ShoppingListEntity
{
    private readonly List<ShoppingListItemEntity> _items = [];

    private ShoppingListEntity()
    {
    }

    public ShoppingListEntity(
        Guid id,
        Guid householdId,
        string name,
        bool isDefault,
        Guid createdByUserId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateTimeOffset? archivedAt)
    {
        Id = id;
        HouseholdId = householdId;
        Name = name;
        IsDefault = isDefault;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        ArchivedAt = archivedAt;
    }

    public Guid Id { get; private set; }
    public Guid HouseholdId { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsDefault { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? ArchivedAt { get; private set; }
    public IReadOnlyCollection<ShoppingListItemEntity> Items => _items;

    public ShoppingList ToDomain()
    {
        return new ShoppingList(Id, HouseholdId, Name, CreatedByUserId, CreatedAt, UpdatedAt, ArchivedAt);
    }

    public static ShoppingListEntity FromDomain(ShoppingList shoppingList)
    {
        ArgumentNullException.ThrowIfNull(shoppingList);

        return new ShoppingListEntity(
            shoppingList.Id,
            shoppingList.HouseholdId,
            shoppingList.Name,
            isDefault: false,
            shoppingList.CreatedByUserId,
            shoppingList.CreatedAt,
            shoppingList.UpdatedAt,
            shoppingList.ArchivedAt);
    }
}
