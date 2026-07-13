using Domu.Api.Features.ShoppingLists.Domain;

namespace Domu.Api.Features.ShoppingLists.Domain.ShoppingLists;

public sealed class ShoppingList
{
    public const int NameMaxLength = 120;

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
    public string Name { get; private set; } = null!;

    public Guid CreatedByMemberId { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? ArchivedAt { get; private set; }

    public void Rename(string name, DateTimeOffset updatedAt)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Shopping list name cannot be empty.", nameof(name));

        var cleanedName = NameNormalizer.Clean(name);
        if (cleanedName.Length > NameMaxLength)
            throw new ArgumentException(
                $"Shopping list name cannot be longer than {NameMaxLength} characters.",
                nameof(name));

        Name = cleanedName;
        UpdatedAt = updatedAt;
    }

    public void Archive(DateTimeOffset archivedAt)
    {
        ArchivedAt = archivedAt;
        UpdatedAt = archivedAt;
    }

    public void Unarchive(DateTimeOffset updatedAt)
    {
        ArchivedAt = null;
        UpdatedAt = updatedAt;
    }
}
