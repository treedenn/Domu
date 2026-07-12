namespace Domu.Api.Features.ShoppingLists.Domain.Items;

public sealed class ShoppingListItem
{
    public const int NameMaxLength = 120;
    public const int UnitMaxLength = 32;
    public const int NoteMaxLength = 500;

    private static readonly HashSet<string> AllowedContainerUnits = new(StringComparer.OrdinalIgnoreCase)
    {
        "pieces",
        "ml",
        "l",
        "mg",
        "g"
    };

    public ShoppingListItem(
        Guid id,
        Guid householdId,
        Guid shoppingListId,
        string name,
        Guid addedByMemberId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        decimal sortOrder)
    {
        Id = id == Guid.Empty
            ? throw new ArgumentException("Shopping list item id cannot be empty.", nameof(id))
            : id;
        HouseholdId = householdId == Guid.Empty
            ? throw new ArgumentException("Household id cannot be empty.", nameof(householdId))
            : householdId;
        ShoppingListId = shoppingListId == Guid.Empty
            ? throw new ArgumentException("Shopping list id cannot be empty.", nameof(shoppingListId))
            : shoppingListId;
        AddedByMemberId = addedByMemberId == Guid.Empty
            ? throw new ArgumentException("Added by member id cannot be empty.", nameof(addedByMemberId))
            : addedByMemberId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        SortOrder = sortOrder;
        Rename(name, updatedAt);
    }

    public Guid Id { get; }
    public Guid HouseholdId { get; }
    public Guid ShoppingListId { get; }
    public string Name { get; private set; } = null!;

    public string NormalizedName { get; private set; } = null!;

    public decimal? Quantity { get; private set; }
    public decimal? ContainerQuantity { get; private set; }
    public string? ContainerUnit { get; private set; }

    public string? Note { get; private set; }

    public bool Checked { get; private set; }
    public DateTimeOffset? CheckedAt { get; private set; }
    public Guid? CheckedByMemberId { get; private set; }
    public Guid? SpaceId { get; private set; }
    public Guid? ItemId { get; private set; }
    public Guid AddedByMemberId { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public decimal SortOrder { get; private set; }

    public void Rename(string name, DateTimeOffset updatedAt)
    {
        var cleanedName = ShoppingListText.CleanName(name);
        if (cleanedName.Length > NameMaxLength)
            throw new ArgumentException(
                $"Shopping list item name cannot be longer than {NameMaxLength} characters.",
                nameof(name));

        Name = cleanedName;
        NormalizedName = ShoppingListText.NormalizeName(cleanedName);
        UpdatedAt = updatedAt;
    }

    public void ChangeQuantity(decimal? quantity, DateTimeOffset updatedAt)
    {
        if (quantity is <= 0)
            throw new ArgumentException("Shopping list item quantity must be greater than 0.", nameof(quantity));

        Quantity = quantity;
        UpdatedAt = updatedAt;
    }

    public void ChangeContainer(decimal? containerQuantity, string? containerUnit, DateTimeOffset updatedAt)
    {
        if (containerQuantity is <= 0)
            throw new ArgumentException("Shopping list item container quantity must be greater than 0.",
                nameof(containerQuantity));

        var cleanedContainerUnit = string.IsNullOrWhiteSpace(containerUnit) ? null : containerUnit.Trim();
        if (cleanedContainerUnit?.Length > UnitMaxLength)
            throw new ArgumentException(
                $"Shopping list item container unit cannot be longer than {UnitMaxLength} characters.",
                nameof(containerUnit));
        if (cleanedContainerUnit is not null && !AllowedContainerUnits.Contains(cleanedContainerUnit))
            throw new ArgumentException("Shopping list item container unit is invalid.", nameof(containerUnit));

        ContainerQuantity = containerQuantity;
        ContainerUnit = cleanedContainerUnit?.ToLowerInvariant();
        UpdatedAt = updatedAt;
    }

    public void ChangeNote(string? note, DateTimeOffset updatedAt)
    {
        var cleanedNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        if (cleanedNote?.Length > NoteMaxLength)
            throw new ArgumentException(
                $"Shopping list item note cannot be longer than {NoteMaxLength} characters.",
                nameof(note));

        Note = cleanedNote;
        UpdatedAt = updatedAt;
    }

    public void LinkSpace(Guid? spaceId, DateTimeOffset updatedAt)
    {
        if (spaceId == Guid.Empty)
            throw new ArgumentException("Space id cannot be empty.", nameof(spaceId));

        SpaceId = spaceId;
        UpdatedAt = updatedAt;
    }

    public void LinkItem(Guid? itemId, DateTimeOffset updatedAt)
    {
        if (itemId == Guid.Empty)
            throw new ArgumentException("Item id cannot be empty.", nameof(itemId));

        ItemId = itemId;
        UpdatedAt = updatedAt;
    }

    public void MoveTo(decimal sortOrder, DateTimeOffset updatedAt)
    {
        SortOrder = sortOrder;
        UpdatedAt = updatedAt;
    }

    public void Check(Guid memberId, DateTimeOffset checkedAt)
    {
        if (memberId == Guid.Empty)
            throw new ArgumentException("Checked by member id cannot be empty.", nameof(memberId));

        Checked = true;
        CheckedAt = checkedAt;
        CheckedByMemberId = memberId;
        UpdatedAt = checkedAt;
    }

    public void Uncheck(DateTimeOffset updatedAt)
    {
        Checked = false;
        CheckedAt = null;
        CheckedByMemberId = null;
        UpdatedAt = updatedAt;
    }
}