using Domu.Api.Features.ShoppingLists.Domain.Items;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.ShoppingLists.Infrastructure.Items;

public sealed class ShoppingListItemEntity
{
    private ShoppingListItemEntity()
    {
    }

    public ShoppingListItemEntity(
        Guid id,
        Guid householdId,
        Guid shoppingListId,
        string name,
        string normalizedName,
        string? note,
        bool @checked,
        DateTimeOffset? checkedAt,
        Guid? checkedByMemberId,
        Guid? spaceId,
        Guid? itemId,
        Guid addedByMemberId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        decimal sortOrder,
        int count = 1,
        decimal? amountPerUnit = null,
        ItemUnit? unit = null,
        DateTimeOffset? submittedToInventoryAt = null,
        Guid? createdInventoryEntryId = null)
    {
        Id = id;
        HouseholdId = householdId;
        ShoppingListId = shoppingListId;
        Name = name;
        NormalizedName = normalizedName;
        Note = note;
        Checked = @checked;
        CheckedAt = checkedAt;
        CheckedByMemberId = checkedByMemberId;
        SpaceId = spaceId;
        ItemId = itemId;
        AddedByMemberId = addedByMemberId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        SortOrder = sortOrder;
        Count = count; AmountPerUnit = amountPerUnit; Unit = unit;
        SubmittedToInventoryAt = submittedToInventoryAt; CreatedInventoryEntryId = createdInventoryEntryId;
    }

    public Guid Id { get; }
    public Guid HouseholdId { get; private set; }
    public Guid ShoppingListId { get; private set; }
    public string Name { get; private set; } = null!;
    public string NormalizedName { get; private set; } = null!;
    public string? Note { get; private set; }
    public bool Checked { get; private set; }
    public DateTimeOffset? CheckedAt { get; private set; }
    public Guid? CheckedByMemberId { get; private set; }
    public Guid? SpaceId { get; private set; }
    public Guid? ItemId { get; private set; }
    public Guid AddedByMemberId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public decimal SortOrder { get; private set; }
    public int Count { get; private set; }
    public decimal? AmountPerUnit { get; private set; }
    public ItemUnit? Unit { get; private set; }
    public DateTimeOffset? SubmittedToInventoryAt { get; private set; }
    public Guid? CreatedInventoryEntryId { get; private set; }

    public ShoppingListItem ToDomain()
    {
        var item = new ShoppingListItem(
            Id,
            HouseholdId,
            ShoppingListId,
            Name,
            AddedByMemberId,
            CreatedAt,
            UpdatedAt,
            SortOrder);

        item.ChangeNote(Note, UpdatedAt);
        item.LinkSpace(SpaceId, UpdatedAt);
        item.LinkItem(ItemId, UpdatedAt);
        item.SetPurchaseDetails(Count, AmountPerUnit, Unit, UpdatedAt);

        if (Checked)
            item.Check(CheckedByMemberId ?? AddedByMemberId, CheckedAt ?? UpdatedAt);
        if (SubmittedToInventoryAt is not null && CreatedInventoryEntryId is not null)
            item.MarkSubmittedToInventory(CreatedInventoryEntryId.Value, SubmittedToInventoryAt.Value);

        return item;
    }

    public static ShoppingListItemEntity FromDomain(ShoppingListItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new ShoppingListItemEntity(
            item.Id,
            item.HouseholdId,
            item.ShoppingListId,
            item.Name,
            item.NormalizedName,
            item.Note,
            item.Checked,
            item.CheckedAt,
            item.CheckedByMemberId,
            item.SpaceId,
            item.ItemId,
            item.AddedByMemberId,
            item.CreatedAt,
            item.UpdatedAt,
            item.SortOrder, item.Count, item.AmountPerUnit, item.Unit, item.SubmittedToInventoryAt, item.CreatedInventoryEntryId);
    }

    public void UpdateFromDomain(ShoppingListItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item.Id != Id)
            throw new ArgumentException("Cannot update shopping list item entity from a different item.", nameof(item));

        HouseholdId = item.HouseholdId;
        ShoppingListId = item.ShoppingListId;
        Name = item.Name;
        NormalizedName = item.NormalizedName;
        Note = item.Note;
        Checked = item.Checked;
        CheckedAt = item.CheckedAt;
        CheckedByMemberId = item.CheckedByMemberId;
        SpaceId = item.SpaceId;
        ItemId = item.ItemId;
        AddedByMemberId = item.AddedByMemberId;
        CreatedAt = item.CreatedAt;
        UpdatedAt = item.UpdatedAt;
        SortOrder = item.SortOrder;
        Count = item.Count; AmountPerUnit = item.AmountPerUnit; Unit = item.Unit;
        SubmittedToInventoryAt = item.SubmittedToInventoryAt; CreatedInventoryEntryId = item.CreatedInventoryEntryId;
    }
}
