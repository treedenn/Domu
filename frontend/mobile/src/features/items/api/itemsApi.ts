import { ApiError, apiRequest, type ApiRequestOptions } from '@/core/http/apiClient';

export enum ItemUnit {
  Unspecified = 'unspecified',
  Piece = 'piece',
  Milliliter = 'milliliter',
  Liter = 'liter',
  Gram = 'gram',
  Kilogram = 'kilogram',
}

export enum ItemContainerType {
  Unspecified = 'unspecified',
  Bottle = 'bottle',
  Carton = 'carton',
  Can = 'can',
  Jar = 'jar',
  Pack = 'pack',
  Box = 'box',
  Bag = 'bag',
}

export enum ConsumableState {
  Unspecified = 'unspecified',
  Unopened = 'unopened',
  Opened = 'opened',
}

export type ItemEntryRequest = {
  id?: string | null;
  initialQuantity: number;
  currentQuantity: number;
  unit?: ItemUnit | null;
  containerType?: ItemContainerType | null;
  state: ConsumableState;
  acquisitionDate?: string | null;
  expirationDate?: string | null;
};

export type ItemEntryView = Required<Omit<ItemEntryRequest, 'id' | 'unit' | 'containerType'>> & {
  id: string;
  unit: ItemUnit;
  containerType: ItemContainerType;
};

export type ItemView = {
  id: string;
  spaceId: string;
  name: string;
  category: string | null;
  barcode: string | null;
  totalQuantity: number;
  entries: ItemEntryView[];
};

export type CreateItemRequest = {
  name: string;
  category?: string | null;
  barcode?: string | null;
  entries?: ItemEntryRequest[] | null;
};

export type UpdateItemRequest = {
  name: string;
  category?: string | null;
  barcode?: string | null;
};

export type ReplaceItemEntriesRequest = {
  entries: ItemEntryRequest[];
};

export function getItems(householdId: string, spaceId: string, options?: ApiRequestOptions) {
  return apiRequest<ItemView[]>(itemsPath(householdId, spaceId), options);
}

export async function getItem(
  householdId: string,
  spaceId: string,
  itemId: string,
  options?: ApiRequestOptions,
) {
  const items = await getItems(householdId, spaceId, options);
  const item = items.find((candidate) => candidate.id === itemId);

  if (!item) {
    throw new ApiError(`Item '${itemId}' was not found.`, 404);
  }

  return item;
}

export function createItem(
  householdId: string,
  spaceId: string,
  request: CreateItemRequest,
  options?: ApiRequestOptions,
) {
  return apiRequest<ItemView>(itemsPath(householdId, spaceId), {
    ...options,
    body: request,
    method: 'POST',
  });
}

export function updateItem(
  householdId: string,
  spaceId: string,
  itemId: string,
  request: UpdateItemRequest,
  options?: ApiRequestOptions,
) {
  return apiRequest<ItemView>(`${itemsPath(householdId, spaceId)}/${itemId}`, {
    ...options,
    body: request,
    method: 'PUT',
  });
}

export function replaceItemEntries(
  householdId: string,
  spaceId: string,
  itemId: string,
  request: ReplaceItemEntriesRequest,
  options?: ApiRequestOptions,
) {
  return apiRequest<ItemView>(`${itemsPath(householdId, spaceId)}/${itemId}/entries`, {
    ...options,
    body: request,
    method: 'PUT',
  });
}

export function deleteItem(
  householdId: string,
  spaceId: string,
  itemId: string,
  options?: ApiRequestOptions,
) {
  return apiRequest<void>(`${itemsPath(householdId, spaceId)}/${itemId}`, {
    ...options,
    method: 'DELETE',
  });
}

function itemsPath(householdId: string, spaceId: string) {
  return `/households/${householdId}/spaces/${spaceId}/items`;
}
