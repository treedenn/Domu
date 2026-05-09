import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { router, useLocalSearchParams } from 'expo-router';
import { useCallback, useEffect, useMemo, useState, type ComponentProps, type ReactNode } from 'react';
import {
  ActivityIndicator,
  Alert,
  Pressable,
  RefreshControl,
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
  deleteItem,
  getItem,
  getItems,
  ItemContainerType,
  ItemUnit,
  replaceItemEntries,
  updateItem,
  type ItemEntryRequest,
  type ItemEntryView,
  type ItemView,
} from '@/features/items/api';
import { AppTopBar, type AppTopBarAction } from '@/ui/AppTopBar';

type QuantitySummary = {
  current: number;
  initial: number;
  label: string;
  unit: ItemUnit;
};

type ItemIdentityFormValue = {
  barcode: string;
  category: string;
  name: string;
};

const emptyEntries: ItemEntryView[] = [];
const entryFormRoute = '/households/[householdId]/items/[itemId]/entry' as never;
const spaceRoute = '/households/[householdId]/spaces' as never;

export default function ItemDetailsScreen() {
  const { householdId, itemId, spaceId } = useLocalSearchParams<{
    householdId?: string | string[];
    itemId?: string | string[];
    spaceId?: string | string[];
  }>();
  const resolvedHouseholdId = firstParam(householdId);
  const resolvedItemId = firstParam(itemId);
  const resolvedSpaceId = firstParam(spaceId);
  const { accessToken, clearTokenResponse } = useAuthSession();
  const [item, setItem] = useState<ItemView | null>(null);
  const [loading, setLoading] = useState(false);
  const [editingItem, setEditingItem] = useState(false);
  const [savingItem, setSavingItem] = useState(false);
  const [deletingItem, setDeletingItem] = useState(false);
  const [deletingEntryId, setDeletingEntryId] = useState<string | null>(null);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const returnToSignIn = useCallback(async () => {
    await clearTokenResponse();
    router.replace('/');
  }, [clearTokenResponse]);

  const loadItem = useCallback(
    async ({ refresh = false } = {}) => {
      if (!accessToken || !resolvedHouseholdId || !resolvedItemId || !resolvedSpaceId) {
        setError('Open an item from a space to view details.');
        setItem(null);
        return;
      }

      if (refresh) {
        setRefreshing(true);
      } else {
        setLoading(true);
      }

      setError(null);

      try {
        const nextItem = await loadItemDetails(
          resolvedHouseholdId,
          resolvedSpaceId,
          resolvedItemId,
          accessToken,
        );
        setItem(nextItem);
      } catch (exception) {
        if (isExpiredSessionError(exception)) {
          await returnToSignIn();
          return;
        }

        setError(getUserFacingError(exception));
      } finally {
        setLoading(false);
        setRefreshing(false);
      }
    },
    [accessToken, resolvedHouseholdId, resolvedItemId, resolvedSpaceId, returnToSignIn],
  );

  useEffect(() => {
    loadItem();
  }, [loadItem]);

  const entries = item?.entries ?? emptyEntries;
  const details = useMemo(() => buildItemDetails(entries), [entries]);
  const quantitySummaries = useMemo(() => summarizeQuantities(entries), [entries]);
  const primarySummary = quantitySummaries[0];
  const canMutate = Boolean(
    accessToken && resolvedHouseholdId && resolvedItemId && resolvedSpaceId && item,
  );

  const returnToSpaceItems = useCallback(() => {
    if (!resolvedHouseholdId || !resolvedSpaceId) {
      router.back();
      return;
    }

    router.dismissTo({
      pathname: spaceRoute,
      params: {
        householdId: resolvedHouseholdId,
        parentId: resolvedSpaceId,
        tab: 'items',
      },
    });
  }, [resolvedHouseholdId, resolvedSpaceId]);

  const deleteCurrentItem = useCallback(async () => {
    if (!accessToken || !resolvedHouseholdId || !resolvedItemId || !resolvedSpaceId) {
      setError('Open an item from a space before deleting it.');
      return;
    }

    setDeletingItem(true);
    setError(null);

    try {
      await deleteItem(resolvedHouseholdId, resolvedSpaceId, resolvedItemId, { accessToken });
      returnToSpaceItems();
    } catch (exception) {
      if (isExpiredSessionError(exception)) {
        await returnToSignIn();
        return;
      }

      setError(getUserFacingError(exception));
    } finally {
      setDeletingItem(false);
    }
  }, [
    accessToken,
    resolvedHouseholdId,
    resolvedItemId,
    resolvedSpaceId,
    returnToSignIn,
    returnToSpaceItems,
  ]);

  const confirmDeleteItem = useCallback(() => {
    Alert.alert(
      'Delete item?',
      item ? `Delete ${item.name} and all of its entries?` : 'Delete this item and all of its entries?',
      [
        { style: 'cancel', text: 'Cancel' },
        { onPress: deleteCurrentItem, style: 'destructive', text: 'Delete' },
      ],
    );
  }, [deleteCurrentItem, item]);

  const topBarActions = useMemo<AppTopBarAction[]>(
    () => [
      {
        disabled: !item,
        icon: editingItem ? 'close' : 'edit',
        label: editingItem ? 'Close editor' : 'Edit item',
        onPress: () => setEditingItem((editing) => !editing),
      },
      {
        disabled: loading || refreshing || !accessToken,
        icon: 'refresh',
        label: 'Refresh',
        onPress: () => loadItem({ refresh: true }),
      },
      {
        destructive: true,
        disabled: !canMutate || deletingItem,
        icon: 'delete-outline',
        label: 'Delete item',
        loading: deletingItem,
        onPress: confirmDeleteItem,
      },
    ],
    [
      accessToken,
      canMutate,
      confirmDeleteItem,
      deletingItem,
      editingItem,
      item,
      loadItem,
      loading,
      refreshing,
    ],
  );

  const deleteEntry = useCallback(
    async (entryId: string) => {
      if (!accessToken || !resolvedHouseholdId || !resolvedItemId || !resolvedSpaceId || !item) {
        setError('Open an item from a space before deleting an entry.');
        return;
      }

      setDeletingEntryId(entryId);
      setError(null);

      try {
        const nextItem = await replaceItemEntries(
          resolvedHouseholdId,
          resolvedSpaceId,
          resolvedItemId,
          {
            entries: item.entries
              .filter((entry) => entry.id !== entryId)
              .map(toItemEntryRequest),
          },
          { accessToken },
        );
        setItem(nextItem);
      } catch (exception) {
        if (isExpiredSessionError(exception)) {
          await returnToSignIn();
          return;
        }

        setError(getUserFacingError(exception));
      } finally {
        setDeletingEntryId(null);
      }
    },
    [accessToken, item, resolvedHouseholdId, resolvedItemId, resolvedSpaceId, returnToSignIn],
  );

  const confirmDeleteEntry = useCallback(
    (entry: ItemEntryView) => {
      Alert.alert(
        'Delete entry?',
        `Delete this ${formatEntryQuantity(entry)} entry from ${item?.name ?? 'this item'}?`,
        [
          { style: 'cancel', text: 'Cancel' },
          { onPress: () => deleteEntry(entry.id), style: 'destructive', text: 'Delete' },
        ],
      );
    },
    [deleteEntry, item?.name],
  );

  const saveItemIdentity = useCallback(
    async (value: ItemIdentityFormValue) => {
      if (!accessToken || !resolvedHouseholdId || !resolvedItemId || !resolvedSpaceId) {
        setError('Open an item from a space before editing it.');
        return;
      }

      const trimmedName = value.name.trim();

      if (!trimmedName) {
        setError('Enter an item name.');
        return;
      }

      setSavingItem(true);
      setError(null);

      try {
        const nextItem = await updateItem(
          resolvedHouseholdId,
          resolvedSpaceId,
          resolvedItemId,
          {
            barcode: value.barcode.trim() || null,
            category: value.category.trim() || null,
            name: trimmedName,
          },
          { accessToken },
        );
        setItem(nextItem);
        setEditingItem(false);
      } catch (exception) {
        if (isExpiredSessionError(exception)) {
          await returnToSignIn();
          return;
        }

        setError(getUserFacingError(exception));
      } finally {
        setSavingItem(false);
      }
    },
    [accessToken, resolvedHouseholdId, resolvedItemId, resolvedSpaceId, returnToSignIn],
  );

  const openEntryForm = useCallback(
    (entryId?: string) => {
      if (!resolvedHouseholdId || !resolvedItemId || !resolvedSpaceId) {
        return;
      }

      router.push({
        pathname: entryFormRoute,
        params: {
          householdId: resolvedHouseholdId,
          itemId: resolvedItemId,
          spaceId: resolvedSpaceId,
          ...(entryId ? { entryId } : {}),
        },
      });
    },
    [resolvedHouseholdId, resolvedItemId, resolvedSpaceId],
  );

  return (
    <SafeAreaView style={styles.safeArea}>
      <ScrollView
        contentContainerStyle={styles.content}
        refreshControl={
          <RefreshControl
            onRefresh={() => loadItem({ refresh: true })}
            refreshing={refreshing}
            tintColor="#526049"
          />
        }>
        <AppTopBar
          actions={topBarActions}
          onBack={() => router.back()}
          subtitle={item?.category || 'Uncategorized'}
          title={item?.name ?? 'Item Details'}
        />

        {item ? (
          <View style={styles.panel}>
            <View style={styles.sectionHeader}>
              <Text style={styles.sectionTitle}>Item Details</Text>
            </View>

            {editingItem ? (
              <ItemIdentityForm
                item={item}
                onCancel={() => setEditingItem(false)}
                onSubmit={saveItemIdentity}
                saving={savingItem}
              />
            ) : (
              <View style={styles.identityGrid}>
                <DetailRow label="Barcode" value={item.barcode || 'Not set'} />
              </View>
            )}
          </View>
        ) : null}

        {loading ? (
          <View style={styles.loadingPanel}>
            <ActivityIndicator color="#526049" />
            <Text style={styles.loadingText}>Loading item details</Text>
          </View>
        ) : null}

        {error ? (
          <View style={styles.errorPanel}>
            <MaterialIcons color="#944931" name="error-outline" size={22} />
            <View style={styles.errorCopy}>
              <Text style={styles.errorTitle}>Could not load item</Text>
              <Text style={styles.errorText}>{error}</Text>
            </View>
          </View>
        ) : null}

        {item ? (
          <>
            <View style={styles.summaryGrid}>
              <SummaryCard
                icon="radio-button-checked"
                label="State"
                tone="primary"
                value={formatState(details.state)}
              />
              <SummaryCard
                icon="shopping-bag"
                label="Bought"
                value={formatDate(details.acquisitionDate)}
              />
              <SummaryCard
                icon="event"
                label="Expires"
                tone={details.isExpired ? 'warning' : undefined}
                value={formatDate(details.expirationDate)}
              />
              <SummaryCard
                icon="inventory-2"
                label="Container"
                value={formatContainer(details.containerType)}
              />
            </View>

            <View style={styles.panel}>
              <View style={styles.sectionHeader}>
                <Text style={styles.sectionTitle}>Amount Left</Text>
                <Text style={styles.sectionMeta}>{formatCount(entries.length, 'entry')}</Text>
              </View>

              {primarySummary ? (
                <View style={styles.amountHero}>
                  <Text style={styles.amountValue}>{formatQuantity(primarySummary)}</Text>
                  <Text style={styles.amountLabel}>
                    {formatPercent(primarySummary.current, primarySummary.initial)} remaining
                  </Text>
                  <View style={styles.progressTrack}>
                    <View
                      style={[
                        styles.progressFill,
                        { width: `${getProgress(primarySummary.current, primarySummary.initial)}%` },
                      ]}
                    />
                  </View>
                </View>
              ) : (
                <Text style={styles.emptyText}>No quantity entries have been registered.</Text>
              )}

              {quantitySummaries.slice(1).map((summary) => (
                <View key={`${summary.label}-${summary.unit}`} style={styles.amountRow}>
                  <Text style={styles.amountRowLabel}>{summary.label}</Text>
                  <Text style={styles.amountRowValue}>{formatQuantity(summary)}</Text>
                </View>
              ))}
            </View>

            <View style={styles.panel}>
              <View style={styles.sectionHeader}>
                <Text style={styles.sectionTitle}>Entries</Text>
                <Pressable
                  accessibilityRole="button"
                  onPress={() => openEntryForm()}
                  style={({ pressed }) => [styles.smallActionButton, pressed && styles.pressed]}>
                  <MaterialIcons color="#526049" name="add" size={18} />
                  <Text style={styles.smallActionText}>Add Entry</Text>
                </Pressable>
              </View>
              <Text style={styles.sectionMeta}>{formatCount(entries.length, 'entry')}</Text>

              <View style={styles.entryList}>
                {entries.map((entry, index) => (
                  <EntryRow
                    deleting={deletingEntryId === entry.id}
                    entry={entry}
                    index={index}
                    key={entry.id}
                    onDelete={confirmDeleteEntry}
                    onEdit={() => openEntryForm(entry.id)}
                  />
                ))}
              </View>
            </View>
          </>
        ) : null}
      </ScrollView>
    </SafeAreaView>
  );
}

async function loadItemDetails(
  householdId: string,
  spaceId: string,
  itemId: string,
  accessToken: string,
) {
  try {
    return await getItem(householdId, spaceId, itemId, { accessToken });
  } catch (exception) {
    if (exception instanceof ApiError && (exception.status === 404 || exception.status === 405)) {
      const items = await getItems(householdId, spaceId, { accessToken });
      const item = items.find((candidate) => candidate.id === itemId);

      if (item) {
        return item;
      }
    }

    throw exception;
  }
}

function SummaryCard({
  icon,
  label,
  tone,
  value,
}: {
  icon: ComponentProps<typeof MaterialIcons>['name'];
  label: string;
  tone?: 'primary' | 'warning';
  value: string;
}) {
  return (
    <View style={styles.summaryCard}>
      <View style={[styles.summaryIcon, tone === 'warning' && styles.summaryIconWarning]}>
        <MaterialIcons
          color={tone === 'warning' ? '#76321c' : '#526049'}
          name={icon}
          size={20}
        />
      </View>
      <Text style={styles.summaryLabel}>{label}</Text>
      <Text numberOfLines={2} style={styles.summaryValue}>
        {value}
      </Text>
    </View>
  );
}

function DetailRow({ label, value }: { label: string; value: string }) {
  return (
    <View style={styles.detailRow}>
      <Text style={styles.detailLabel}>{label}</Text>
      <Text numberOfLines={2} style={styles.detailValue}>
        {value}
      </Text>
    </View>
  );
}

function ItemIdentityForm({
  item,
  onCancel,
  onSubmit,
  saving,
}: {
  item: ItemView;
  onCancel: () => void;
  onSubmit: (value: ItemIdentityFormValue) => void;
  saving: boolean;
}) {
  const [name, setName] = useState(item.name);
  const [category, setCategory] = useState(item.category ?? '');
  const [barcode, setBarcode] = useState(item.barcode ?? '');
  const canSave = Boolean(name.trim()) && !saving;

  useEffect(() => {
    setName(item.name);
    setCategory(item.category ?? '');
    setBarcode(item.barcode ?? '');
  }, [item]);

  const submit = useCallback(() => {
    onSubmit({ barcode, category, name });
  }, [barcode, category, name, onSubmit]);

  return (
    <View style={styles.formStack}>
      <FormField label="Name">
        <TextInput
          autoCapitalize="words"
          onChangeText={setName}
          placeholder="Item name"
          placeholderTextColor="#8c8a81"
          style={styles.input}
          value={name}
        />
      </FormField>
      <FormField label="Category">
        <TextInput
          autoCapitalize="words"
          onChangeText={setCategory}
          placeholder="Kitchen"
          placeholderTextColor="#8c8a81"
          style={styles.input}
          value={category}
        />
      </FormField>
      <FormField label="Barcode">
        <TextInput
          autoCapitalize="none"
          keyboardType="number-pad"
          onChangeText={setBarcode}
          placeholder="Barcode"
          placeholderTextColor="#8c8a81"
          style={styles.input}
          value={barcode}
        />
      </FormField>
      <FormActions
        canSave={canSave}
        onCancel={onCancel}
        onSave={submit}
        saving={saving}
      />
    </View>
  );
}

function EntryRow({
  deleting,
  entry,
  index,
  onDelete,
  onEdit,
}: {
  deleting: boolean;
  entry: ItemEntryView;
  index: number;
  onDelete: (entry: ItemEntryView) => void;
  onEdit: () => void;
}) {
  return (
    <View style={styles.entryRow}>
      <View style={styles.entryNumber}>
        <Text style={styles.entryNumberText}>{index + 1}</Text>
      </View>
      <View style={styles.entryContent}>
        <View style={styles.entryTopRow}>
          <Text style={styles.entryTitle}>{formatEntryQuantity(entry)}</Text>
          <View style={styles.entryActions}>
            <Text style={styles.entryState}>{formatState(entry.state)}</Text>
            <Pressable
              accessibilityLabel="Edit entry"
              accessibilityRole="button"
              onPress={onEdit}
              style={({ pressed }) => [styles.entryEditButton, pressed && styles.pressed]}>
              <MaterialIcons color="#526049" name="edit" size={18} />
            </Pressable>
            <Pressable
              accessibilityLabel="Delete entry"
              accessibilityRole="button"
              disabled={deleting}
              onPress={() => onDelete(entry)}
              style={({ pressed }) => [
                styles.entryDeleteButton,
                pressed && styles.pressed,
                deleting && styles.disabledButton,
              ]}>
              {deleting ? (
                <ActivityIndicator color="#944931" size="small" />
              ) : (
                <MaterialIcons color="#944931" name="delete-outline" size={18} />
              )}
            </Pressable>
          </View>
        </View>
        <View style={styles.entryMetaGrid}>
          <MetaPill icon="shopping-bag" label={formatDate(entry.acquisitionDate)} />
          <MetaPill icon="event" label={formatDate(entry.expirationDate)} />
          <MetaPill icon="inventory-2" label={formatContainer(entry.containerType)} />
        </View>
      </View>
    </View>
  );
}

function FormField({ children, label }: { children: ReactNode; label: string }) {
  return (
    <View style={styles.field}>
      <Text style={styles.label}>{label}</Text>
      {children}
    </View>
  );
}

function FormActions({
  canSave,
  onCancel,
  onSave,
  saving,
}: {
  canSave: boolean;
  onCancel: () => void;
  onSave: () => void;
  saving: boolean;
}) {
  return (
    <View style={styles.formActions}>
      <Pressable
        accessibilityRole="button"
        onPress={onCancel}
        style={({ pressed }) => [styles.cancelButton, pressed && styles.pressed]}>
        <Text style={styles.cancelButtonText}>Cancel</Text>
      </Pressable>
      <Pressable
        accessibilityRole="button"
        disabled={!canSave}
        onPress={onSave}
        style={({ pressed }) => [
          styles.saveButton,
          pressed && styles.pressed,
          !canSave && styles.disabledButton,
        ]}>
        {saving ? (
          <ActivityIndicator color="#ffffff" />
        ) : (
          <>
            <MaterialIcons color="#ffffff" name="save" size={18} />
            <Text style={styles.saveButtonText}>Save</Text>
          </>
        )}
      </Pressable>
    </View>
  );
}

function toItemEntryRequest(entry: ItemEntryView): ItemEntryRequest {
  return {
    acquisitionDate: entry.acquisitionDate,
    containerType: entry.containerType,
    currentQuantity: entry.currentQuantity,
    expirationDate: entry.expirationDate,
    id: entry.id,
    initialQuantity: entry.initialQuantity,
    state: entry.state,
    unit: entry.unit,
  };
}

function MetaPill({
  icon,
  label,
}: {
  icon: ComponentProps<typeof MaterialIcons>['name'];
  label: string;
}) {
  return (
    <View style={styles.metaPill}>
      <MaterialIcons color="#757870" name={icon} size={14} />
      <Text numberOfLines={1} style={styles.metaPillText}>
        {label}
      </Text>
    </View>
  );
}

function buildItemDetails(entries: ItemEntryView[]) {
  const acquisitionDate = sortDates(entries.map((entry) => entry.acquisitionDate), 'asc')[0] ?? null;
  const expirationDate = sortDates(entries.map((entry) => entry.expirationDate), 'asc')[0] ?? null;
  const openedEntry = entries.find((entry) => entry.state === ConsumableState.Opened);
  const state = openedEntry?.state ?? entries[0]?.state ?? ConsumableState.Unspecified;
  const containerType = getMostCommon(entries.map((entry) => entry.containerType));

  return {
    acquisitionDate,
    containerType,
    expirationDate,
    isExpired: expirationDate ? new Date(expirationDate).getTime() < startOfToday().getTime() : false,
    state,
  };
}

function summarizeQuantities(entries: ItemEntryView[]): QuantitySummary[] {
  const summaries = new Map<string, QuantitySummary>();

  for (const entry of entries) {
    const normalized = normalizeQuantity(entry.currentQuantity, entry.initialQuantity, entry.unit);
    const current = summaries.get(normalized.label) ?? {
      current: 0,
      initial: 0,
      label: normalized.label,
      unit: normalized.unit,
    };

    summaries.set(normalized.label, {
      ...current,
      current: current.current + normalized.current,
      initial: current.initial + normalized.initial,
    });
  }

  return Array.from(summaries.values()).sort((left, right) => left.label.localeCompare(right.label));
}

function normalizeQuantity(current: number, initial: number, unit: ItemUnit) {
  if (unit === ItemUnit.Liter) {
    return { current: current * 1000, initial: initial * 1000, label: 'Volume left', unit: ItemUnit.Milliliter };
  }

  if (unit === ItemUnit.Kilogram) {
    return { current: current * 1000, initial: initial * 1000, label: 'Mass left', unit: ItemUnit.Gram };
  }

  if (unit === ItemUnit.Milliliter) {
    return { current, initial, label: 'Volume left', unit };
  }

  if (unit === ItemUnit.Gram) {
    return { current, initial, label: 'Mass left', unit };
  }

  return { current, initial, label: 'Quantity left', unit: ItemUnit.Piece };
}

function sortDates(values: (string | null)[], direction: 'asc' | 'desc') {
  return values
    .filter((value): value is string => Boolean(value))
    .sort((left, right) => {
      const result = new Date(left).getTime() - new Date(right).getTime();
      return direction === 'asc' ? result : -result;
    });
}

function getMostCommon<T>(values: T[]) {
  const counts = new Map<T, number>();

  for (const value of values) {
    counts.set(value, (counts.get(value) ?? 0) + 1);
  }

  return Array.from(counts.entries()).sort((left, right) => right[1] - left[1])[0]?.[0];
}

function firstParam(value?: string | string[]) {
  return Array.isArray(value) ? value[0] : value;
}

function formatCount(value: number, noun: string) {
  return `${value} ${noun}${value === 1 ? '' : 's'}`;
}

function formatDate(value?: string | null) {
  if (!value) {
    return 'Not set';
  }

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return 'Not set';
  }

  return new Intl.DateTimeFormat(undefined, {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  }).format(date);
}

function formatEntryQuantity(entry: ItemEntryView) {
  return `${formatNumber(entry.currentQuantity)} / ${formatNumber(entry.initialQuantity)} ${formatUnit(entry.unit)}`;
}

function formatQuantity(summary: QuantitySummary) {
  return `${formatNumber(summary.current)} / ${formatNumber(summary.initial)} ${formatUnit(summary.unit)}`;
}

function formatNumber(value: number) {
  return Number.isInteger(value) ? String(value) : value.toLocaleString(undefined, { maximumFractionDigits: 1 });
}

function formatPercent(current: number, initial: number) {
  if (initial <= 0) {
    return '0%';
  }

  return `${Math.round((current / initial) * 100)}%`;
}

function getProgress(current: number, initial: number) {
  if (initial <= 0) {
    return 0;
  }

  return Math.max(0, Math.min(100, Math.round((current / initial) * 100)));
}

function formatUnit(unit: ItemUnit) {
  switch (unit) {
    case ItemUnit.Milliliter:
      return 'ml';
    case ItemUnit.Liter:
      return 'l';
    case ItemUnit.Gram:
      return 'g';
    case ItemUnit.Kilogram:
      return 'kg';
    case ItemUnit.Piece:
      return 'pcs';
    case ItemUnit.Unspecified:
    default:
      return 'pcs';
  }
}

function formatState(state: ConsumableState) {
  switch (state) {
    case ConsumableState.Opened:
      return 'Opened';
    case ConsumableState.Unopened:
      return 'Unopened';
    case ConsumableState.Unspecified:
    default:
      return 'Unknown';
  }
}

function formatContainer(containerType?: ItemContainerType) {
  switch (containerType) {
    case ItemContainerType.Bag:
      return 'Bag';
    case ItemContainerType.Bottle:
      return 'Bottle';
    case ItemContainerType.Box:
      return 'Box';
    case ItemContainerType.Can:
      return 'Can';
    case ItemContainerType.Carton:
      return 'Carton';
    case ItemContainerType.Jar:
      return 'Jar';
    case ItemContainerType.Pack:
      return 'Pack';
    case ItemContainerType.Unspecified:
    default:
      return 'Not set';
  }
}

function startOfToday() {
  const date = new Date();
  date.setHours(0, 0, 0, 0);
  return date;
}

function getUserFacingError(exception: unknown) {
  if (exception instanceof TimeoutError) {
    return `${exception.message} Check adb reverse or EXPO_PUBLIC_API_URL.`;
  }

  if (exception instanceof ApiError) {
    if (exception.status === 401) {
      return 'Your session is missing or expired. Sign in again.';
    }

    if (exception.status === 404) {
      return 'This item was not found.';
    }

    return exception.message;
  }

  return 'Check that the backend is running at the configured API URL.';
}

function isExpiredSessionError(exception: unknown) {
  return exception instanceof ApiError && exception.status === 401;
}

const styles = StyleSheet.create({
  safeArea: {
    backgroundColor: '#fff8f3',
    flex: 1,
  },
  content: {
    gap: 20,
    padding: 20,
    paddingBottom: 36,
  },
  topBar: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 10,
    justifyContent: 'space-between',
  },
  topTitleBlock: {
    alignItems: 'center',
    flex: 1,
    gap: 2,
    minWidth: 0,
  },
  topTitle: {
    color: '#1e1b18',
    fontSize: 16,
    fontWeight: '800',
    letterSpacing: 0,
  },
  topSubtitle: {
    color: '#757870',
    fontSize: 12,
    fontWeight: '700',
  },
  topActions: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 8,
  },
  iconButton: {
    alignItems: 'center',
    backgroundColor: '#faf2ed',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    height: 44,
    justifyContent: 'center',
    width: 44,
  },
  dangerIconButton: {
    backgroundColor: '#ffdbd0',
    borderColor: '#ffb59e',
  },
  disabledButton: {
    opacity: 0.5,
  },
  identityGrid: {
    gap: 10,
  },
  detailRow: {
    alignItems: 'flex-start',
    borderBottomColor: '#e8e1dc',
    borderBottomWidth: 1,
    flexDirection: 'row',
    gap: 12,
    justifyContent: 'space-between',
    paddingBottom: 10,
  },
  detailLabel: {
    color: '#757870',
    fontSize: 13,
    fontWeight: '800',
  },
  detailValue: {
    color: '#1e1b18',
    flex: 1,
    fontSize: 14,
    fontWeight: '800',
    textAlign: 'right',
  },
  loadingPanel: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 10,
  },
  loadingText: {
    color: '#444841',
    fontSize: 14,
    fontWeight: '700',
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
  errorCopy: {
    flex: 1,
    gap: 6,
  },
  errorTitle: {
    color: '#3a0b00',
    fontSize: 15,
    fontWeight: '800',
  },
  errorText: {
    color: '#76321c',
    fontSize: 14,
    lineHeight: 20,
  },
  summaryGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 10,
  },
  summaryCard: {
    backgroundColor: '#ffffff',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    flexGrow: 1,
    gap: 7,
    minHeight: 126,
    minWidth: '47%',
    padding: 14,
    shadowColor: '#5c5854',
    shadowOffset: { height: 4, width: 0 },
    shadowOpacity: 0.05,
    shadowRadius: 12,
  },
  summaryIcon: {
    alignItems: 'center',
    backgroundColor: '#d8e8cb',
    borderRadius: 8,
    height: 38,
    justifyContent: 'center',
    width: 38,
  },
  summaryIconWarning: {
    backgroundColor: '#ffdbd0',
  },
  summaryLabel: {
    color: '#757870',
    fontSize: 12,
    fontWeight: '800',
    textTransform: 'uppercase',
  },
  summaryValue: {
    color: '#1e1b18',
    fontSize: 17,
    fontWeight: '800',
    lineHeight: 22,
  },
  panel: {
    backgroundColor: '#ffffff',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    gap: 16,
    padding: 16,
    shadowColor: '#5c5854',
    shadowOffset: { height: 4, width: 0 },
    shadowOpacity: 0.05,
    shadowRadius: 12,
  },
  sectionHeader: {
    alignItems: 'center',
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  sectionTitle: {
    color: '#1e1b18',
    fontSize: 20,
    fontWeight: '800',
  },
  sectionMeta: {
    color: '#757870',
    fontSize: 13,
    fontWeight: '700',
  },
  smallActionButton: {
    alignItems: 'center',
    backgroundColor: '#faf2ed',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    gap: 6,
    justifyContent: 'center',
    minHeight: 38,
    paddingHorizontal: 10,
  },
  smallActionText: {
    color: '#526049',
    fontSize: 13,
    fontWeight: '800',
  },
  formStack: {
    gap: 14,
  },
  field: {
    flex: 1,
    gap: 8,
  },
  label: {
    color: '#444841',
    fontSize: 13,
    fontWeight: '800',
  },
  input: {
    backgroundColor: '#faf2ed',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    color: '#1e1b18',
    fontSize: 15,
    minHeight: 46,
    paddingHorizontal: 12,
  },
  formActions: {
    flexDirection: 'row',
    gap: 10,
  },
  cancelButton: {
    alignItems: 'center',
    backgroundColor: '#ffffff',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    flex: 1,
    justifyContent: 'center',
    minHeight: 46,
  },
  cancelButtonText: {
    color: '#444841',
    fontSize: 14,
    fontWeight: '800',
  },
  saveButton: {
    alignItems: 'center',
    backgroundColor: '#526049',
    borderRadius: 8,
    flex: 1,
    flexDirection: 'row',
    gap: 6,
    justifyContent: 'center',
    minHeight: 46,
  },
  saveButtonText: {
    color: '#ffffff',
    fontSize: 14,
    fontWeight: '800',
  },
  amountHero: {
    backgroundColor: '#faf2ed',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    gap: 8,
    padding: 14,
  },
  amountValue: {
    color: '#1e1b18',
    fontSize: 26,
    fontWeight: '800',
  },
  amountLabel: {
    color: '#444841',
    fontSize: 13,
    fontWeight: '700',
  },
  progressTrack: {
    backgroundColor: '#e8e1dc',
    borderRadius: 999,
    height: 10,
    overflow: 'hidden',
  },
  progressFill: {
    backgroundColor: '#526049',
    borderRadius: 999,
    height: '100%',
  },
  emptyText: {
    color: '#444841',
    fontSize: 14,
    lineHeight: 20,
  },
  amountRow: {
    alignItems: 'center',
    borderTopColor: '#e8e1dc',
    borderTopWidth: 1,
    flexDirection: 'row',
    justifyContent: 'space-between',
    paddingTop: 12,
  },
  amountRowLabel: {
    color: '#444841',
    fontSize: 13,
    fontWeight: '800',
  },
  amountRowValue: {
    color: '#1e1b18',
    fontSize: 14,
    fontWeight: '800',
  },
  entryList: {
    gap: 12,
  },
  entryRow: {
    alignItems: 'flex-start',
    backgroundColor: '#faf2ed',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    gap: 12,
    padding: 12,
  },
  entryNumber: {
    alignItems: 'center',
    backgroundColor: '#d8e8cb',
    borderRadius: 8,
    height: 34,
    justifyContent: 'center',
    width: 34,
  },
  entryNumberText: {
    color: '#121f0d',
    fontSize: 14,
    fontWeight: '800',
  },
  entryContent: {
    flex: 1,
    gap: 8,
    minWidth: 0,
  },
  entryTopRow: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 8,
    justifyContent: 'space-between',
  },
  entryTitle: {
    color: '#1e1b18',
    flex: 1,
    fontSize: 15,
    fontWeight: '800',
  },
  entryState: {
    color: '#526049',
    fontSize: 12,
    fontWeight: '800',
  },
  entryActions: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 8,
  },
  entryEditButton: {
    alignItems: 'center',
    backgroundColor: '#ffffff',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    height: 34,
    justifyContent: 'center',
    width: 34,
  },
  entryDeleteButton: {
    alignItems: 'center',
    backgroundColor: '#ffdbd0',
    borderColor: '#ffb59e',
    borderRadius: 8,
    borderWidth: 1,
    height: 34,
    justifyContent: 'center',
    width: 34,
  },
  entryMetaGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  metaPill: {
    alignItems: 'center',
    backgroundColor: '#ffffff',
    borderRadius: 8,
    flexDirection: 'row',
    gap: 4,
    minHeight: 28,
    paddingHorizontal: 8,
  },
  metaPillText: {
    color: '#444841',
    fontSize: 12,
    fontWeight: '700',
    maxWidth: 112,
  },
  pressed: {
    opacity: 0.78,
  },
});
