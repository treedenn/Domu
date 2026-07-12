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
        Guid createdByMemberId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateTimeOffset? archivedAt)
    {
        Id = id;
        HouseholdId = householdId;
        Name = name;
        IsDefault = isDefault;
        CreatedByMemberId = createdByMemberId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        ArchivedAt = archivedAt;
    }

    public Guid Id { get; }
    public Guid HouseholdId { get; }
    public string Name { get; } = null!;
    public bool IsDefault { get; private set; }
    public Guid CreatedByMemberId { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; }
    public DateTimeOffset? ArchivedAt { get; }
    public IReadOnlyCollection<ShoppingListItemEntity> Items => _items;

    public ShoppingList ToDomain()
    {
        return new ShoppingList(Id, HouseholdId, Name, CreatedByMemberId, CreatedAt, UpdatedAt, ArchivedAt);
    }

    public static ShoppingListEntity FromDomain(ShoppingList shoppingList)
    {
        ArgumentNullException.ThrowIfNull(shoppingList);

        return new ShoppingListEntity(
            shoppingList.Id,
            shoppingList.HouseholdId,
            shoppingList.Name,
            false,
            shoppingList.CreatedByMemberId,
            shoppingList.CreatedAt,
            shoppingList.UpdatedAt,
            shoppingList.ArchivedAt);
    }
}