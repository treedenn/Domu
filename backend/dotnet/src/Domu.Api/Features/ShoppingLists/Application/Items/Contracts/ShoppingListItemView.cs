using Domu.Api.Features.ShoppingLists.Domain.Items;

namespace Domu.Api.Features.ShoppingLists.Application.Items.Contracts;

public sealed record ShoppingListItemView(
    Guid Id,
    Guid HouseholdId,
    Guid ShoppingListId,
    string Name,
    string NormalizedName,
    decimal? Quantity,
    decimal? ContainerQuantity,
    string? ContainerUnit,
    string? Note,
    bool Checked,
    DateTimeOffset? CheckedAt,
    Guid? CheckedByUserId,
    Guid? SpaceId,
    Guid? ItemId,
    Guid AddedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    decimal SortOrder)
{
    public static ShoppingListItemView FromDomain(ShoppingListItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new ShoppingListItemView(
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
            item.CheckedByUserId,
            item.SpaceId,
            item.ItemId,
            item.AddedByUserId,
            item.CreatedAt,
            item.UpdatedAt,
            item.SortOrder);
    }
}
