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

    private string _name = null!;
    private string _normalizedName = null!;
    private string? _containerUnit;
    private string? _note;

    public ShoppingListItem(
        Guid id,
        Guid householdId,
        Guid shoppingListId,
        string name,
        Guid addedByUserId,
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
        AddedByUserId = addedByUserId == Guid.Empty
            ? throw new ArgumentException("Added by user id cannot be empty.", nameof(addedByUserId))
            : addedByUserId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        SortOrder = sortOrder;
        Rename(name, updatedAt);
    }

    public Guid Id { get; }
    public Guid HouseholdId { get; }
    public Guid ShoppingListId { get; }
    public string Name => _name;
    public string NormalizedName => _normalizedName;
    public decimal? Quantity { get; private set; }
    public decimal? ContainerQuantity { get; private set; }
    public string? ContainerUnit => _containerUnit;
    public string? Note => _note;
    public bool Checked { get; private set; }
    public DateTimeOffset? CheckedAt { get; private set; }
    public Guid? CheckedByUserId { get; private set; }
    public Guid? SpaceId { get; private set; }
    public Guid? ItemId { get; private set; }
    public Guid AddedByUserId { get; }
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

        _name = cleanedName;
        _normalizedName = ShoppingListText.NormalizeName(cleanedName);
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
            throw new ArgumentException("Shopping list item container quantity must be greater than 0.", nameof(containerQuantity));

        var cleanedContainerUnit = string.IsNullOrWhiteSpace(containerUnit) ? null : containerUnit.Trim();
        if (cleanedContainerUnit?.Length > UnitMaxLength)
            throw new ArgumentException(
                $"Shopping list item container unit cannot be longer than {UnitMaxLength} characters.",
                nameof(containerUnit));
        if (cleanedContainerUnit is not null && !AllowedContainerUnits.Contains(cleanedContainerUnit))
            throw new ArgumentException("Shopping list item container unit is invalid.", nameof(containerUnit));

        ContainerQuantity = containerQuantity;
        _containerUnit = cleanedContainerUnit?.ToLowerInvariant();
        UpdatedAt = updatedAt;
    }

    public void ChangeNote(string? note, DateTimeOffset updatedAt)
    {
        var cleanedNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        if (cleanedNote?.Length > NoteMaxLength)
            throw new ArgumentException(
                $"Shopping list item note cannot be longer than {NoteMaxLength} characters.",
                nameof(note));

        _note = cleanedNote;
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

    public void Check(Guid userId, DateTimeOffset checkedAt)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Checked by user id cannot be empty.", nameof(userId));

        Checked = true;
        CheckedAt = checkedAt;
        CheckedByUserId = userId;
        UpdatedAt = checkedAt;
    }

    public void Uncheck(DateTimeOffset updatedAt)
    {
        Checked = false;
        CheckedAt = null;
        CheckedByUserId = null;
        UpdatedAt = updatedAt;
    }
}
