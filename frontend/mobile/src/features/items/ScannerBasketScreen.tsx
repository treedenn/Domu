import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { router, useLocalSearchParams } from 'expo-router';
import { useCallback, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { TimeoutError } from '@/core/async/timeout';
import { ApiError } from '@/core/http/apiClient';
import { useAuthSession } from '@/features/auth/authSession';
import { createItem } from '@/features/items/api';
import { PendingScannerItemCard } from '@/features/items/scanner/PendingScannerItemCard';
import {
  createPlaceholderScannerItem,
  parsePendingItems,
  toCreateItemRequest,
  type PendingScannerItem,
} from '@/features/items/scanner/scannerBasketModel';
import { AppTopBar } from '@/ui/AppTopBar';

const spaceRoute = '/households/[householdId]/spaces' as never;

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
    setPendingItems((currentItems) => [...currentItems, createPlaceholderScannerItem()]);
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
            <PendingScannerItemCard
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
