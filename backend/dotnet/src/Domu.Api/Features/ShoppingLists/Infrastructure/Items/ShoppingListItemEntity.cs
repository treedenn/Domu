using Domu.Api.Features.ShoppingLists.Domain.Items;

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
        decimal? quantity,
        decimal? containerQuantity,
        string? containerUnit,
        string? note,
        bool @checked,
        DateTimeOffset? checkedAt,
        Guid? checkedByMemberId,
        Guid? spaceId,
        Guid? itemId,
        Guid addedByMemberId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        decimal sortOrder)
    {
        Id = id;
        HouseholdId = householdId;
        ShoppingListId = shoppingListId;
        Name = name;
        NormalizedName = normalizedName;
        Quantity = quantity;
        ContainerQuantity = containerQuantity;
        ContainerUnit = containerUnit;
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
    }

    public Guid Id { get; private set; }
    public Guid HouseholdId { get; private set; }
    public Guid ShoppingListId { get; private set; }
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
    public Guid AddedByMemberId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public decimal SortOrder { get; private set; }

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

        item.ChangeQuantity(Quantity, UpdatedAt);
        item.ChangeContainer(ContainerQuantity, ContainerUnit, UpdatedAt);
        item.ChangeNote(Note, UpdatedAt);
        item.LinkSpace(SpaceId, UpdatedAt);
        item.LinkItem(ItemId, UpdatedAt);

        if (Checked)
            item.Check(CheckedByMemberId ?? AddedByMemberId, CheckedAt ?? UpdatedAt);

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
            item.Quantity,
            item.ContainerQuantity,
            item.ContainerUnit,
            item.Note,
            item.Checked,
            item.CheckedAt,
            item.CheckedByMemberId,
            item.SpaceId,
            item.ItemId,
            item.AddedByMemberId,
            item.CreatedAt,
            item.UpdatedAt,
            item.SortOrder);
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
        Quantity = item.Quantity;
        ContainerQuantity = item.ContainerQuantity;
        ContainerUnit = item.ContainerUnit;
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
    }
}
