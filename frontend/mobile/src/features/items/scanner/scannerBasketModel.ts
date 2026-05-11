import {
  ConsumableState,
  ItemContainerType,
  ItemUnit,
  type CreateItemRequest,
} from '@/features/items/api';

export type PendingScannerItem = {
  barcode: string | null;
  category: string;
  imageUri: string | null;
  name: string;
  quantity: number;
  source: 'camera' | 'photo';
};

export function createPlaceholderScannerItem(): PendingScannerItem {
  return {
    barcode: null,
    category: 'Kitchen',
    imageUri: null,
    name: '',
    quantity: 1,
    source: 'camera',
  };
}

export function toCreateItemRequest(item: PendingScannerItem): CreateItemRequest {
  return {
    barcode: item.barcode,
    category: item.category.trim() || null,
    entries: [
      {
        containerType: ItemContainerType.Unspecified,
        currentQuantity: item.quantity,
        initialQuantity: item.quantity,
        state: ConsumableState.Unopened,
        unit: ItemUnit.Piece,
      },
    ],
    name: item.name.trim(),
  };
}

export function parsePendingItems(value?: string): PendingScannerItem[] {
  if (!value) {
    return [];
  }

  try {
    const parsed = JSON.parse(value) as Partial<PendingScannerItem>[];

    if (!Array.isArray(parsed)) {
      return [];
    }

    return parsed.map((item) => ({
      barcode: typeof item.barcode === 'string' ? item.barcode : null,
      category: typeof item.category === 'string' ? item.category : 'Kitchen',
      imageUri: typeof item.imageUri === 'string' ? item.imageUri : null,
      name: typeof item.name === 'string' ? item.name : '',
      quantity: typeof item.quantity === 'number' ? item.quantity : 1,
      source: item.source === 'photo' ? 'photo' : 'camera',
    }));
  } catch {
    return [];
  }
}
