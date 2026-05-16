import { apiRequest, type ApiRequestOptions } from '@/core/http/apiClient';

export type ShoppingListView = {
  id: string;
  householdId: string;
  name: string;
  isDefault: boolean;
  createdByUserId: string;
  createdAt: string;
  updatedAt: string;
  archivedAt: string | null;
};

export type ShoppingListItemView = {
  id: string;
  householdId: string;
  shoppingListId: string;
  name: string;
  normalizedName: string;
  quantity: number | null;
  containerQuantity: number | null;
  containerUnit: string | null;
  note: string | null;
  checked: boolean;
  checkedAt: string | null;
  checkedByUserId: string | null;
  spaceId: string | null;
  itemId: string | null;
  addedByUserId: string;
  createdAt: string;
  updatedAt: string;
  sortOrder: number;
};

export type CreateShoppingListItemRequest = {
  name: string;
  quantity?: number | null;
  containerQuantity?: number | null;
  containerUnit?: string | null;
  note?: string | null;
  spaceId?: string | null;
  itemId?: string | null;
};

export type UpdateShoppingListItemRequest = {
  name?: string | null;
  quantity?: number | null;
  containerQuantity?: number | null;
  containerUnit?: string | null;
  note?: string | null;
  spaceId?: string | null;
  itemId?: string | null;
  sortOrder?: number | null;
};

export function getDefaultShoppingList(householdId: string, options?: ApiRequestOptions) {
  return apiRequest<ShoppingListView>(`/households/${householdId}/shopping-list/default`, options);
}

export function getShoppingListItems(
  householdId: string,
  shoppingListId: string,
  options?: ApiRequestOptions,
) {
  return apiRequest<ShoppingListItemView[]>(
    shoppingListItemsPath(householdId, shoppingListId),
    options,
  );
}

export function createShoppingListItem(
  householdId: string,
  shoppingListId: string,
  request: CreateShoppingListItemRequest,
  options?: ApiRequestOptions,
) {
  return apiRequest<ShoppingListItemView>(shoppingListItemsPath(householdId, shoppingListId), {
    ...options,
    body: request,
    method: 'POST',
  });
}

export function updateShoppingListItem(
  householdId: string,
  shoppingListId: string,
  itemId: string,
  request: UpdateShoppingListItemRequest,
  options?: ApiRequestOptions,
) {
  return apiRequest<ShoppingListItemView>(
    `${shoppingListItemsPath(householdId, shoppingListId)}/${itemId}`,
    {
      ...options,
      body: request,
      method: 'PATCH',
    },
  );
}

export function checkShoppingListItem(
  householdId: string,
  shoppingListId: string,
  itemId: string,
  options?: ApiRequestOptions,
) {
  return apiRequest<ShoppingListItemView>(
    `${shoppingListItemsPath(householdId, shoppingListId)}/${itemId}/check`,
    {
      ...options,
      method: 'POST',
    },
  );
}

export function uncheckShoppingListItem(
  householdId: string,
  shoppingListId: string,
  itemId: string,
  options?: ApiRequestOptions,
) {
  return apiRequest<ShoppingListItemView>(
    `${shoppingListItemsPath(householdId, shoppingListId)}/${itemId}/uncheck`,
    {
      ...options,
      method: 'POST',
    },
  );
}

export function deleteShoppingListItem(
  householdId: string,
  shoppingListId: string,
  itemId: string,
  options?: ApiRequestOptions,
) {
  return apiRequest<void>(`${shoppingListItemsPath(householdId, shoppingListId)}/${itemId}`, {
    ...options,
    method: 'DELETE',
  });
}

export function clearCheckedShoppingListItems(
  householdId: string,
  shoppingListId: string,
  options?: ApiRequestOptions,
) {
  return apiRequest<void>(`${shoppingListItemsPath(householdId, shoppingListId)}/checked`, {
    ...options,
    method: 'DELETE',
  });
}

function shoppingListItemsPath(householdId: string, shoppingListId: string) {
  return `/households/${householdId}/shopping-lists/${shoppingListId}/items`;
}
