namespace Domu.Api.Features.Events.Application;

public static class HouseholdEventActions
{
    public const string HouseholdCreated = "household.created";
    public const string HouseholdUpdated = "household.updated";
    public const string HouseholdDeleted = "household.deleted";
    public const string HouseholdMemberInvited = "household_member.invited";
    public const string HouseholdInvitationAccepted = "household_invitation.accepted";
    public const string SpaceCreated = "space.created";
    public const string SpaceUpdated = "space.updated";
    public const string SpaceMoved = "space.moved";
    public const string SpaceDeleted = "space.deleted";
    public const string ItemCreated = "item.created";
    public const string ItemUpdated = "item.updated";
    public const string ItemEntriesReplaced = "item.entries_replaced";
    public const string ItemDeleted = "item.deleted";
    public const string ShoppingListCreated = "shopping_list.created";
    public const string ShoppingListUpdated = "shopping_list.updated";
    public const string ShoppingListDeleted = "shopping_list.deleted";
    public const string ShoppingListItemCreated = "shopping_list_item.created";
    public const string ShoppingListItemUpdated = "shopping_list_item.updated";
    public const string ShoppingListItemChecked = "shopping_list_item.checked";
    public const string ShoppingListItemUnchecked = "shopping_list_item.unchecked";
    public const string ShoppingListItemDeleted = "shopping_list_item.deleted";
    public const string ShoppingListCheckedItemsCleared = "shopping_list.checked_items_cleared";
}
