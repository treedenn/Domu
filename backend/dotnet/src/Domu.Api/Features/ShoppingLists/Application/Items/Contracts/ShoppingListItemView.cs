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
    Guid? CheckedByMemberId,
    Guid? SpaceId,
    Guid? ItemId,
    Guid AddedByMemberId,
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
            item.CheckedByMemberId,
            item.SpaceId,
            item.ItemId,
            item.AddedByMemberId,
            item.CreatedAt,
            item.UpdatedAt,
            item.SortOrder);
    }
}