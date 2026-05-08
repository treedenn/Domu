import { apiRequest, type ApiRequestOptions } from '@/core/http/apiClient';
import type { ItemEntryView } from '@/features/items/api';

export type SpaceSearchResultView = {
  id: string;
  householdId: string;
  parentId: string | null;
  name: string;
  description: string | null;
};

export type ItemSearchResultView = {
  id: string;
  spaceId: string;
  name: string;
  category: string | null;
  barcode: string | null;
  totalQuantity: number;
  entries: ItemEntryView[];
};

export type SearchResultsView = {
  spaces: SpaceSearchResultView[];
  items: ItemSearchResultView[];
};

export type SearchQuery = {
  text?: string | null;
  expiringWithinDays?: number | null;
  limit?: number;
};

export function searchHousehold(
  householdId: string,
  query: SearchQuery,
  options?: ApiRequestOptions,
) {
  return apiRequest<SearchResultsView>(`/households/${householdId}/search`, {
    ...options,
    query,
  });
}

