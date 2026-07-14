import { apiRequest, type ApiRequestOptions } from '@/core/http/apiClient';

export type ShoppingListView = {
  id: string;
  householdId: string;
  name: string;
  createdByMemberId: string;
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
  checkedByMemberId: string | null;
  spaceId: string | null;
  itemId: string | null;
  addedByMemberId: string;
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

export type CreateShoppingListRequest = {
  name: string;
};

export type UpdateShoppingListRequest = {
  name: string;
  archived: boolean;
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

export function getShoppingLists(householdId: string, options?: ApiRequestOptions) {
  return apiRequest<ShoppingListView[]>(shoppingListsPath(householdId), options);
}

export function getShoppingList(
  householdId: string,
  shoppingListId: string,
  options?: ApiRequestOptions,
) {
  return apiRequest<ShoppingListView>(
    `${shoppingListsPath(householdId)}/${shoppingListId}`,
    options,
  );
}

export function createShoppingList(
  householdId: string,
  request: CreateShoppingListRequest,
  options?: ApiRequestOptions,
) {
  return apiRequest<ShoppingListView>(shoppingListsPath(householdId), {
    ...options,
    body: request,
    method: 'POST',
  });
}

export function updateShoppingList(
  householdId: string,
  shoppingListId: string,
  request: UpdateShoppingListRequest,
  options?: ApiRequestOptions,
) {
  return apiRequest<ShoppingListView>(`${shoppingListsPath(householdId)}/${shoppingListId}`, {
    ...options,
    body: request,
    method: 'PUT',
  });
}

export function deleteShoppingList(
  householdId: string,
  shoppingListId: string,
  options?: ApiRequestOptions,
) {
  return apiRequest<void>(`${shoppingListsPath(householdId)}/${shoppingListId}`, {
    ...options,
    method: 'DELETE',
  });
}

export async function getOrCreatePrimaryShoppingList(
  householdId: string,
  options?: ApiRequestOptions,
) {
  const lists = await getShoppingLists(householdId, options);
  return lists[0] ?? createShoppingList(householdId, { name: 'Shopping list' }, options);
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

function shoppingListsPath(householdId: string) {
  return `/households/${householdId}/shopping-lists`;
}
