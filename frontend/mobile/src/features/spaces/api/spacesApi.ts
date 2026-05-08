import { apiRequest, type ApiRequestOptions } from '@/core/http/apiClient';

export type CollectionView<T> = {
  count: number;
  data: T[] | null;
};

export type SpaceChildView = {
  id: string;
  householdId: string;
  parentId: string | null;
  name: string;
  description: string | null;
};

export type SpaceItemView = {
  id: string;
  spaceId: string;
  name: string;
  category: string | null;
  barcode: string | null;
  totalQuantity: number;
};

export type SpaceView = {
  id: string;
  householdId: string;
  parentId: string | null;
  name: string;
  description: string | null;
  items: CollectionView<SpaceItemView> | null;
  childSpaces: CollectionView<SpaceChildView> | null;
};

export type SpacePage = {
  spaces: SpaceView[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
};

export type GetSpacesQuery = {
  parentId?: string | null;
  pageNumber?: number;
  pageSize?: number;
  includeItems?: boolean;
  includeItemCount?: boolean;
  includeChildSpaces?: boolean;
  includeChildSpaceCount?: boolean;
};

export type CreateSpaceRequest = {
  name: string;
  description?: string | null;
  parentId?: string | null;
};

export type UpdateSpaceRequest = {
  name: string;
  description?: string | null;
};

export type MoveSpaceRequest = {
  parentId?: string | null;
};

export function getSpaces(
  householdId: string,
  query?: GetSpacesQuery,
  options?: ApiRequestOptions,
) {
  return apiRequest<SpacePage>(spacesPath(householdId), {
    ...options,
    query,
  });
}

export function getSpace(householdId: string, spaceId: string, options?: ApiRequestOptions) {
  return apiRequest<SpaceView>(`${spacesPath(householdId)}/${spaceId}`, options);
}

export function createSpace(
  householdId: string,
  request: CreateSpaceRequest,
  options?: ApiRequestOptions,
) {
  return apiRequest<SpaceView>(spacesPath(householdId), {
    ...options,
    body: request,
    method: 'POST',
  });
}

export function updateSpace(
  householdId: string,
  spaceId: string,
  request: UpdateSpaceRequest,
  options?: ApiRequestOptions,
) {
  return apiRequest<SpaceView>(`${spacesPath(householdId)}/${spaceId}`, {
    ...options,
    body: request,
    method: 'PUT',
  });
}

export function moveSpace(
  householdId: string,
  spaceId: string,
  request: MoveSpaceRequest,
  options?: ApiRequestOptions,
) {
  return apiRequest<SpaceView>(`${spacesPath(householdId)}/${spaceId}/parent`, {
    ...options,
    body: request,
    method: 'PUT',
  });
}

export function deleteSpace(householdId: string, spaceId: string, options?: ApiRequestOptions) {
  return apiRequest<void>(`${spacesPath(householdId)}/${spaceId}`, {
    ...options,
    method: 'DELETE',
  });
}

function spacesPath(householdId: string) {
  return `/households/${householdId}/spaces`;
}

