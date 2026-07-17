using Domu.Api.Features.ShoppingLists.Domain;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.ShoppingLists.Domain.Items;

public sealed class ShoppingListItem
{
    public const int NameMaxLength = 120;
    public const int NoteMaxLength = 500;

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

    public string? Note { get; private set; }

    public bool Checked { get; private set; }
    public DateTimeOffset? CheckedAt { get; private set; }
    public Guid? CheckedByMemberId { get; private set; }
    public Guid? SpaceId { get; private set; }
    public Guid? ItemId { get; private set; }
    public int Count { get; private set; } = 1;
    public decimal? PlannedAmountPerUnit { get; private set; }
    public ItemUnit? PlannedUnit { get; private set; }
    public DateTimeOffset? SubmittedToInventoryAt { get; private set; }
    public Guid? CreatedInventoryEntryId { get; private set; }
    public Guid AddedByMemberId { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public decimal SortOrder { get; private set; }

    public void Rename(string name, DateTimeOffset updatedAt)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Shopping list item name cannot be empty.", nameof(name));

        var cleanedName = NameNormalizer.Clean(name);
        if (cleanedName.Length > NameMaxLength)
            throw new ArgumentException(
                $"Shopping list item name cannot be longer than {NameMaxLength} characters.",
                nameof(name));

        Name = cleanedName;
        NormalizedName = NameNormalizer.NormalizeForComparison(cleanedName);
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

    public void SetPlannedBatch(int count, decimal? amountPerUnit, ItemUnit? unit, DateTimeOffset updatedAt)
    {
        if (count <= 0)
            throw new ArgumentException("Shopping list item count must be greater than 0.", nameof(count));
        if (amountPerUnit.HasValue != unit.HasValue || amountPerUnit < 0 || (unit.HasValue && (!Enum.IsDefined(unit.Value) || unit == ItemUnit.Unspecified)))
            throw new ArgumentException("Shopping list planned amount and unit must be supplied together with a specified unit.");
        Count = count; PlannedAmountPerUnit = amountPerUnit; PlannedUnit = unit; UpdatedAt = updatedAt;
    }

    public void MarkSubmittedToInventory(Guid entryId, DateTimeOffset submittedAt)
    {
        if (entryId == Guid.Empty) throw new ArgumentException("Inventory entry id cannot be empty.", nameof(entryId));
        if (SubmittedToInventoryAt is not null) throw new InvalidOperationException("Shopping list item has already been submitted to inventory.");
        SubmittedToInventoryAt = submittedAt; CreatedInventoryEntryId = entryId; UpdatedAt = submittedAt;
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
