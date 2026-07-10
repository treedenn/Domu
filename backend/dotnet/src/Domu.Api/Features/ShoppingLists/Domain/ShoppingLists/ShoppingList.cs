using Domu.Api.Features.ShoppingLists.Domain.Items;

namespace Domu.Api.Features.ShoppingLists.Domain.ShoppingLists;

public sealed class ShoppingList
{
    public const int NameMaxLength = 120;

    private string _name = null!;

    public ShoppingList(
        Guid id,
        Guid householdId,
        string name,
        Guid createdByMemberId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateTimeOffset? archivedAt = null)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("Shopping list id cannot be empty.", nameof(id))
            : id;
        HouseholdId = householdId == Guid.Empty
            ? throw new ArgumentException("Household id cannot be empty.", nameof(householdId))
            : householdId;
        CreatedByMemberId = createdByMemberId == Guid.Empty
            ? throw new ArgumentException("Created by member id cannot be empty.", nameof(createdByMemberId))
            : createdByMemberId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        ArchivedAt = archivedAt;
        Rename(name, updatedAt);
    }

    public Guid Id { get; }
    public Guid HouseholdId { get; }
    public string Name => _name;
    public Guid CreatedByMemberId { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? ArchivedAt { get; private set; }

    public void Rename(string name, DateTimeOffset updatedAt)
    {
        var cleanedName = ShoppingListText.CleanName(name);
        if (cleanedName.Length > NameMaxLength)
            throw new ArgumentException(
                $"Shopping list name cannot be longer than {NameMaxLength} characters.",
                nameof(name));

        _name = cleanedName;
        UpdatedAt = updatedAt;
    }

    public void Archive(DateTimeOffset archivedAt)
    {
        ArchivedAt = archivedAt;
        UpdatedAt = archivedAt;
    }
}
