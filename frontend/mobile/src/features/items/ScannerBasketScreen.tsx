import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { router, useLocalSearchParams } from 'expo-router';
import { useCallback, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  Image,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { TimeoutError } from '@/core/async/timeout';
import { ApiError } from '@/core/http/apiClient';
import { useAuthSession } from '@/features/auth/authSession';
import {
  ConsumableState,
  createItem,
  ItemContainerType,
  ItemUnit,
  type CreateItemRequest,
} from '@/features/items/api';
import { AppTopBar } from '@/ui/AppTopBar';

const spaceRoute = '/households/[householdId]/spaces' as never;

type PendingScannerItem = {
  barcode: string | null;
  category: string;
  imageUri: string | null;
  name: string;
  quantity: number;
  source: 'camera' | 'photo';
};

export default function ScannerBasketScreen() {
  const { householdId, spaceId, items } = useLocalSearchParams<{
    householdId?: string | string[];
    spaceId?: string | string[];
    items?: string | string[];
  }>();
  const resolvedHouseholdId = firstParam(householdId);
  const resolvedSpaceId = firstParam(spaceId);
  const initialItems = useMemo(() => parsePendingItems(firstParam(items)), [items]);
  const [pendingItems, setPendingItems] = useState<PendingScannerItem[]>(initialItems);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { accessToken, clearTokenResponse } = useAuthSession();

  const returnToSignIn = useCallback(async () => {
    await clearTokenResponse();
    router.replace('/');
  }, [clearTokenResponse]);

  const updateItem = useCallback((index: number, value: PendingScannerItem) => {
    setPendingItems((currentItems) =>
      currentItems.map((item, itemIndex) => (itemIndex === index ? value : item)),
    );
  }, []);

  const removeItem = useCallback((index: number) => {
    setPendingItems((currentItems) => currentItems.filter((_, itemIndex) => itemIndex !== index));
  }, []);

  const addPlaceholderItem = useCallback(() => {
    setPendingItems((currentItems) => [
      ...currentItems,
      {
        barcode: null,
        category: 'Kitchen',
        imageUri: null,
        name: '',
        quantity: 1,
        source: 'camera',
      },
    ]);
  }, []);

  const finalizeAll = useCallback(async () => {
    if (!accessToken || !resolvedHouseholdId || !resolvedSpaceId) {
      setError('Open a space before adding items.');
      return;
    }

    const validItems = pendingItems.filter((item) => item.name.trim());

    if (!validItems.length) {
      setError('Add at least one item name before finalizing.');
      return;
    }

    setSaving(true);
    setError(null);

    try {
      await Promise.all(
        validItems.map((item) =>
          createItem(resolvedHouseholdId, resolvedSpaceId, toCreateItemRequest(item), {
            accessToken,
          }),
        ),
      );
      router.dismissTo({
        pathname: spaceRoute,
        params: {
          householdId: resolvedHouseholdId,
          parentId: resolvedSpaceId,
          tab: 'items',
        },
      });
    } catch (exception) {
      if (isExpiredSessionError(exception)) {
        await returnToSignIn();
        return;
      }

      setError(getUserFacingError(exception));
    } finally {
      setSaving(false);
    }
  }, [accessToken, pendingItems, resolvedHouseholdId, resolvedSpaceId, returnToSignIn]);

  return (
    <SafeAreaView style={styles.safeArea}>
      <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
        <AppTopBar onBack={() => router.back()} subtitle="Review" title="Scanner Basket" />
        <View style={styles.header}>
          <Text style={styles.title}>Scanner Basket</Text>
          <Text style={styles.body}>
            Review and organize your scanned items before adding them to this space.
          </Text>
        </View>

        <View style={styles.batchBar}>
          <View style={styles.batchMeta}>
            <MaterialIcons color="#944931" name="inventory" size={22} />
            <Text style={styles.batchText}>{formatCount(pendingItems.length, 'item')} Pending</Text>
          </View>
          <Pressable
            accessibilityRole="button"
            disabled={saving || !pendingItems.length}
            onPress={finalizeAll}
            style={({ pressed }) => [
              styles.finalizeButton,
              (saving || !pendingItems.length) && styles.finalizeButtonDisabled,
              pressed && styles.pressed,
            ]}>
            {saving ? (
              <ActivityIndicator color="#ffffff" />
            ) : (
              <Text style={styles.finalizeButtonText}>Finalize All</Text>
            )}
          </Pressable>
        </View>

        {error ? (
          <View style={styles.errorPanel}>
            <MaterialIcons color="#944931" name="error-outline" size={22} />
            <Text style={styles.errorText}>{error}</Text>
          </View>
        ) : null}

        <View style={styles.cards}>
          {pendingItems.map((item, index) => (
            <PendingItemCard
              index={index}
              item={item}
              key={`${item.source}-${index}`}
              onChange={updateItem}
              onRemove={removeItem}
            />
          ))}

          <Pressable
            accessibilityRole="button"
            onPress={addPlaceholderItem}
            style={({ pressed }) => [styles.scanAnotherCard, pressed && styles.pressed]}>
            <View style={styles.scanAnotherIcon}>
              <MaterialIcons color="#444841" name="add-a-photo" size={26} />
            </View>
            <Text style={styles.scanAnotherText}>Scan another item</Text>
          </Pressable>
        </View>

        <View style={styles.suggestionPanel}>
          <MaterialIcons color="#526049" name="lightbulb" size={28} />
          <View style={styles.suggestionCopy}>
            <Text style={styles.suggestionTitle}>Smart Suggestions</Text>
            <Text style={styles.suggestionText}>
              Items finalized from this basket are assigned to the current space automatically.
            </Text>
          </View>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

function PendingItemCard({
  index,
  item,
  onChange,
  onRemove,
}: {
  index: number;
  item: PendingScannerItem;
  onChange: (index: number, item: PendingScannerItem) => void;
  onRemove: (index: number) => void;
}) {
  const setName = useCallback(
    (name: string) => {
      onChange(index, { ...item, name });
    },
    [index, item, onChange],
  );

  const setCategory = useCallback(
    (category: string) => {
      onChange(index, { ...item, category });
    },
    [index, item, onChange],
  );

  const setBarcode = useCallback(
    (barcode: string) => {
      onChange(index, { ...item, barcode: barcode.trim() || null });
    },
    [index, item, onChange],
  );

  const setQuantity = useCallback(
    (quantity: string) => {
      onChange(index, { ...item, quantity: Math.max(Number(quantity) || 1, 1) });
    },
    [index, item, onChange],
  );

  return (
    <View style={styles.itemCard}>
      <View style={styles.itemVisual}>
        {item.imageUri ? (
          <Image source={{ uri: item.imageUri }} style={styles.itemImage} />
        ) : (
          <MaterialIcons
            color={item.source === 'photo' ? '#494740' : '#526049'}
            name={item.source === 'photo' ? 'photo-library' : 'qr-code-scanner'}
            size={30}
          />
        )}
      </View>
      <View style={styles.itemContent}>
        <View style={styles.itemTopRow}>
          <Text style={styles.itemSource}>{item.source === 'photo' ? 'Photo scan' : 'Barcode scan'}</Text>
          <Pressable
            accessibilityLabel="Remove item"
            accessibilityRole="button"
            onPress={() => onRemove(index)}
            style={({ pressed }) => [styles.smallIconButton, pressed && styles.pressed]}>
            <MaterialIcons color="#757870" name="close" size={18} />
          </Pressable>
        </View>

        <TextInput
          autoCapitalize="words"
          onChangeText={setName}
          placeholder="Item name"
          placeholderTextColor="#8c8a81"
          style={styles.cardInput}
          value={item.name}
        />
        <View style={styles.cardInputRow}>
          <TextInput
            onChangeText={setCategory}
            placeholder="Category"
            placeholderTextColor="#8c8a81"
            style={[styles.cardInput, styles.cardInputHalf]}
            value={item.category}
          />
          <TextInput
            keyboardType="number-pad"
            onChangeText={setQuantity}
            placeholder="Qty"
            placeholderTextColor="#8c8a81"
            style={[styles.cardInput, styles.quantityCardInput]}
            value={String(item.quantity)}
          />
        </View>
        <TextInput
          autoCapitalize="none"
          keyboardType="number-pad"
          onChangeText={setBarcode}
          placeholder="Barcode"
          placeholderTextColor="#8c8a81"
          style={styles.cardInput}
          value={item.barcode ?? ''}
        />
      </View>
    </View>
  );
}

function toCreateItemRequest(item: PendingScannerItem): CreateItemRequest {
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

function parsePendingItems(value?: string): PendingScannerItem[] {
  if (!value) {
    return [];
  }

  try {
    const parsed = JSON.parse(value) as Partial<PendingScannerItem>[];

    if (!Array.isArray(parsed)) {
      return [];
    }

    return parsed.map((item, index) => ({
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

function firstParam(value?: string | string[]) {
  return Array.isArray(value) ? value[0] : value;
}

function formatCount(value: number, noun: string) {
  return `${value} ${noun}${value === 1 ? '' : 's'}`;
}

function getUserFacingError(exception: unknown) {
  if (exception instanceof TimeoutError) {
    return `${exception.message} Check adb reverse or EXPO_PUBLIC_API_URL.`;
  }

  if (exception instanceof ApiError) {
    return exception.message;
  }

  return 'Check that the backend is running at the configured API URL.';
}

function isExpiredSessionError(exception: unknown) {
  return exception instanceof ApiError && exception.status === 401;
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: '#fff8f3',
  },
  topBar: {
    alignItems: 'center',
    backgroundColor: '#fff8f3',
    borderBottomColor: '#e8e1dc',
    borderBottomWidth: 1,
    flexDirection: 'row',
    justifyContent: 'space-between',
    paddingHorizontal: 20,
    paddingVertical: 14,
  },
  iconButton: {
    alignItems: 'center',
    borderRadius: 8,
    height: 44,
    justifyContent: 'center',
    width: 44,
  },
  topTitle: {
    color: '#526049',
    fontSize: 24,
    fontWeight: '700',
  },
  content: {
    gap: 18,
    padding: 20,
    paddingBottom: 36,
  },
  header: {
    gap: 6,
    marginBottom: 6,
  },
  title: {
    color: '#526049',
    fontSize: 24,
    fontWeight: '700',
  },
  body: {
    color: '#444841',
    fontSize: 16,
    lineHeight: 24,
  },
  batchBar: {
    alignItems: 'center',
    backgroundColor: '#faf2ed',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    justifyContent: 'space-between',
    gap: 12,
    padding: 14,
  },
  batchMeta: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 8,
  },
  batchText: {
    color: '#444841',
    fontSize: 13,
    fontWeight: '700',
  },
  finalizeButton: {
    alignItems: 'center',
    backgroundColor: '#526049',
    borderRadius: 999,
    justifyContent: 'center',
    minHeight: 40,
    minWidth: 108,
    paddingHorizontal: 16,
  },
  finalizeButtonDisabled: {
    backgroundColor: '#9ca58f',
  },
  finalizeButtonText: {
    color: '#ffffff',
    fontSize: 13,
    fontWeight: '800',
  },
  cards: {
    gap: 12,
  },
  itemCard: {
    backgroundColor: '#ffffff',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    gap: 14,
    padding: 14,
    shadowColor: '#5c5854',
    shadowOffset: { height: 4, width: 0 },
    shadowOpacity: 0.05,
    shadowRadius: 12,
  },
  itemVisual: {
    alignItems: 'center',
    backgroundColor: '#f4ede7',
    borderRadius: 8,
    height: 86,
    justifyContent: 'center',
    overflow: 'hidden',
    width: 86,
  },
  itemImage: {
    height: '100%',
    width: '100%',
  },
  itemContent: {
    flex: 1,
    gap: 8,
    minWidth: 0,
  },
  itemTopRow: {
    alignItems: 'center',
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  itemSource: {
    color: '#757870',
    fontSize: 12,
    fontWeight: '700',
  },
  smallIconButton: {
    alignItems: 'center',
    height: 28,
    justifyContent: 'center',
    width: 28,
  },
  cardInput: {
    backgroundColor: '#faf2ed',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    color: '#1e1b18',
    fontSize: 14,
    minHeight: 42,
    paddingHorizontal: 10,
  },
  cardInputRow: {
    flexDirection: 'row',
    gap: 8,
  },
  cardInputHalf: {
    flex: 1,
  },
  quantityCardInput: {
    width: 64,
    textAlign: 'center',
  },
  scanAnotherCard: {
    alignItems: 'center',
    backgroundColor: '#f4ede7',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderStyle: 'dashed',
    borderWidth: 2,
    gap: 8,
    justifyContent: 'center',
    minHeight: 118,
    padding: 16,
  },
  scanAnotherIcon: {
    alignItems: 'center',
    backgroundColor: '#e8e1dc',
    borderRadius: 999,
    height: 48,
    justifyContent: 'center',
    width: 48,
  },
  scanAnotherText: {
    color: '#444841',
    fontSize: 13,
    fontWeight: '700',
  },
  suggestionPanel: {
    alignItems: 'flex-start',
    backgroundColor: '#e7e2d9',
    borderColor: '#cac6be',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    gap: 12,
    padding: 16,
  },
  suggestionCopy: {
    flex: 1,
    gap: 4,
  },
  suggestionTitle: {
    color: '#1d1c16',
    fontSize: 15,
    fontWeight: '800',
  },
  suggestionText: {
    color: '#494740',
    fontSize: 13,
    lineHeight: 19,
  },
  errorPanel: {
    alignItems: 'flex-start',
    backgroundColor: '#ffdbd0',
    borderColor: '#ffb59e',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    gap: 12,
    padding: 14,
  },
  errorText: {
    color: '#76321c',
    flex: 1,
    fontSize: 14,
    lineHeight: 20,
  },
  pressed: {
    opacity: 0.78,
  },
});
