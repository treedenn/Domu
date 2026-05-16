import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { router, useLocalSearchParams } from 'expo-router';
import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  FlatList,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  RefreshControl,
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
  checkShoppingListItem,
  clearCheckedShoppingListItems,
  createShoppingListItem,
  deleteShoppingListItem,
  getDefaultShoppingList,
  getShoppingListItems,
  type ShoppingListItemView,
  type ShoppingListView,
  uncheckShoppingListItem,
  updateShoppingListItem,
} from '@/features/shopping-lists/api';
import { AppTopBar, type AppTopBarAction } from '@/ui/AppTopBar';

type DraftValue = {
  containerQuantity: string;
  containerUnit: string;
  name: string;
  note: string;
  quantity: string;
};

const emptyItems: ShoppingListItemView[] = [];
const unitOptions = [
  { label: 'No unit', value: '' },
  { label: 'Pieces', value: 'pieces' },
  { label: 'ml', value: 'ml' },
  { label: 'l', value: 'l' },
  { label: 'mg', value: 'mg' },
  { label: 'g', value: 'g' },
];

export default function ShoppingListScreen() {
  const { householdId } = useLocalSearchParams<{ householdId?: string | string[] }>();
  const resolvedHouseholdId = firstParam(householdId);
  const { accessToken, clearTokenResponse } = useAuthSession();
  const [shoppingList, setShoppingList] = useState<ShoppingListView | null>(null);
  const [items, setItems] = useState<ShoppingListItemView[]>(emptyItems);
  const [quickAddName, setQuickAddName] = useState('');
  const [editingItemId, setEditingItemId] = useState<string | null>(null);
  const [editingDraft, setEditingDraft] = useState<DraftValue>(emptyDraft());
  const [loading, setLoading] = useState(false);
  const [refreshing, setRefreshing] = useState(false);
  const [adding, setAdding] = useState(false);
  const [savingItemId, setSavingItemId] = useState<string | null>(null);
  const [deletingItemId, setDeletingItemId] = useState<string | null>(null);
  const [togglingItemId, setTogglingItemId] = useState<string | null>(null);
  const [clearingChecked, setClearingChecked] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const returnToSignIn = useCallback(async () => {
    await clearTokenResponse();
    router.replace('/');
  }, [clearTokenResponse]);

  const loadShoppingList = useCallback(
    async ({ refresh = false } = {}) => {
      if (!accessToken || !resolvedHouseholdId) {
        setError('Sign in and choose a household to view the shopping list.');
        setShoppingList(null);
        setItems(emptyItems);
        return;
      }

      if (refresh) {
        setRefreshing(true);
      } else {
        setLoading(true);
      }

      setError(null);

      try {
        const nextList = await getDefaultShoppingList(resolvedHouseholdId, { accessToken });
        const nextItems = await getShoppingListItems(resolvedHouseholdId, nextList.id, {
          accessToken,
        });

        setShoppingList(nextList);
        setItems(nextItems);
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
    [accessToken, resolvedHouseholdId, returnToSignIn],
  );

  useEffect(() => {
    loadShoppingList();
  }, [loadShoppingList]);

  const activeCount = useMemo(() => items.filter((item) => !item.checked).length, [items]);
  const checkedCount = items.length - activeCount;
  const canMutate = Boolean(accessToken && resolvedHouseholdId && shoppingList);

  const addQuickItem = useCallback(async () => {
    const trimmedName = quickAddName.trim();

    if (!accessToken || !resolvedHouseholdId || !shoppingList || !trimmedName) {
      return;
    }

    setAdding(true);
    setError(null);

    try {
      const item = await createShoppingListItem(
        resolvedHouseholdId,
        shoppingList.id,
        {
          itemId: null,
          name: trimmedName,
          note: null,
          quantity: null,
          containerQuantity: null,
          containerUnit: null,
          spaceId: null,
        },
        { accessToken },
      );

      setItems((currentItems) => [item, ...currentItems]);
      setQuickAddName('');
    } catch (exception) {
      if (isExpiredSessionError(exception)) {
        await returnToSignIn();
        return;
      }

      setError(getUserFacingError(exception));
    } finally {
      setAdding(false);
    }
  }, [accessToken, quickAddName, resolvedHouseholdId, returnToSignIn, shoppingList]);

  const toggleChecked = useCallback(
    async (item: ShoppingListItemView) => {
      if (!accessToken || !resolvedHouseholdId || !shoppingList) {
        return;
      }

      setTogglingItemId(item.id);
      setError(null);

      try {
        const nextItem = item.checked
          ? await uncheckShoppingListItem(resolvedHouseholdId, shoppingList.id, item.id, {
              accessToken,
            })
          : await checkShoppingListItem(resolvedHouseholdId, shoppingList.id, item.id, {
              accessToken,
            });

        setItems((currentItems) => replaceItem(currentItems, nextItem));
      } catch (exception) {
        if (isExpiredSessionError(exception)) {
          await returnToSignIn();
          return;
        }

        setError(getUserFacingError(exception));
      } finally {
        setTogglingItemId(null);
      }
    },
    [accessToken, resolvedHouseholdId, returnToSignIn, shoppingList],
  );

  const startEditing = useCallback((item: ShoppingListItemView) => {
    setEditingItemId(item.id);
    setEditingDraft({
      containerQuantity: item.containerQuantity === null ? '' : String(item.containerQuantity),
      containerUnit: item.containerUnit ?? '',
      name: item.name,
      note: item.note ?? '',
      quantity: item.quantity === null ? '' : String(item.quantity),
    });
  }, []);

  const cancelEditing = useCallback(() => {
    setEditingItemId(null);
    setEditingDraft(emptyDraft());
  }, []);

  const saveEditing = useCallback(
    async (item: ShoppingListItemView) => {
      if (!accessToken || !resolvedHouseholdId || !shoppingList) {
        return;
      }

      const trimmedName = editingDraft.name.trim();
      const parsedQuantity = parseOptionalQuantity(editingDraft.quantity);
      const parsedContainerQuantity = parseOptionalQuantity(editingDraft.containerQuantity);

      if (!trimmedName) {
        setError('Enter an item name.');
        return;
      }

      if (parsedQuantity === undefined) {
        setError('Quantity must be a number greater than zero.');
        return;
      }

      if (parsedContainerQuantity === undefined) {
        setError('Container quantity must be a number greater than zero.');
        return;
      }

      setSavingItemId(item.id);
      setError(null);

      try {
        const nextItem = await updateShoppingListItem(
          resolvedHouseholdId,
          shoppingList.id,
          item.id,
          {
            itemId: item.itemId,
            name: trimmedName,
            note: editingDraft.note.trim() || null,
            quantity: parsedQuantity,
            containerQuantity: parsedContainerQuantity,
            containerUnit: editingDraft.containerUnit || null,
            sortOrder: item.sortOrder,
            spaceId: item.spaceId,
          },
          { accessToken },
        );

        setItems((currentItems) => replaceItem(currentItems, nextItem));
        cancelEditing();
      } catch (exception) {
        if (isExpiredSessionError(exception)) {
          await returnToSignIn();
          return;
        }

        setError(getUserFacingError(exception));
      } finally {
        setSavingItemId(null);
      }
    },
    [
      accessToken,
      cancelEditing,
      editingDraft,
      resolvedHouseholdId,
      returnToSignIn,
      shoppingList,
    ],
  );

  const deleteItem = useCallback(
    async (item: ShoppingListItemView) => {
      if (!accessToken || !resolvedHouseholdId || !shoppingList) {
        return;
      }

      setDeletingItemId(item.id);
      setError(null);

      try {
        await deleteShoppingListItem(resolvedHouseholdId, shoppingList.id, item.id, {
          accessToken,
        });
        setItems((currentItems) => currentItems.filter((candidate) => candidate.id !== item.id));
      } catch (exception) {
        if (isExpiredSessionError(exception)) {
          await returnToSignIn();
          return;
        }

        setError(getUserFacingError(exception));
      } finally {
        setDeletingItemId(null);
      }
    },
    [accessToken, resolvedHouseholdId, returnToSignIn, shoppingList],
  );

  const confirmDeleteItem = useCallback(
    (item: ShoppingListItemView) => {
      Alert.alert('Delete item?', `Delete ${item.name} from the shopping list?`, [
        { style: 'cancel', text: 'Cancel' },
        { onPress: () => deleteItem(item), style: 'destructive', text: 'Delete' },
      ]);
    },
    [deleteItem],
  );

  const clearChecked = useCallback(async () => {
    if (!accessToken || !resolvedHouseholdId || !shoppingList || checkedCount === 0) {
      return;
    }

    setClearingChecked(true);
    setError(null);

    try {
      await clearCheckedShoppingListItems(resolvedHouseholdId, shoppingList.id, {
        accessToken,
      });
      setItems((currentItems) => currentItems.filter((item) => !item.checked));
    } catch (exception) {
      if (isExpiredSessionError(exception)) {
        await returnToSignIn();
        return;
      }

      setError(getUserFacingError(exception));
    } finally {
      setClearingChecked(false);
    }
  }, [
    accessToken,
    checkedCount,
    resolvedHouseholdId,
    returnToSignIn,
    shoppingList,
  ]);

  const confirmClearChecked = useCallback(() => {
    Alert.alert('Clear checked?', `Remove ${formatCount(checkedCount, 'checked item')}?`, [
      { style: 'cancel', text: 'Cancel' },
      { onPress: clearChecked, style: 'destructive', text: 'Clear' },
    ]);
  }, [checkedCount, clearChecked]);

  const topBarActions = useMemo<AppTopBarAction[]>(
    () => [
      {
        disabled: loading || refreshing || !accessToken,
        icon: 'refresh',
        label: 'Refresh',
        onPress: () => loadShoppingList({ refresh: true }),
      },
      {
        destructive: true,
        disabled: !canMutate || checkedCount === 0 || clearingChecked,
        icon: 'delete-sweep',
        label: 'Clear checked',
        loading: clearingChecked,
        onPress: confirmClearChecked,
      },
    ],
    [
      accessToken,
      canMutate,
      checkedCount,
      clearingChecked,
      confirmClearChecked,
      loadShoppingList,
      loading,
      refreshing,
    ],
  );

  return (
    <SafeAreaView style={styles.safeArea}>
      <KeyboardAvoidingView
        behavior={Platform.select({ ios: 'padding', default: undefined })}
        style={styles.screen}>
        <FlatList
          ListHeaderComponent={
            <View style={styles.headerContent}>
              <AppTopBar
                actions={topBarActions}
                onBack={() => router.back()}
                subtitle="Household"
                title={shoppingList?.name ?? 'Shopping List'}
              />

              <View style={styles.summaryGrid}>
                <SummaryCard icon="shopping-bag" label="To buy" value={activeCount} />
                <SummaryCard icon="check-circle-outline" label="Checked" value={checkedCount} />
              </View>

              <View style={styles.quickAddPanel}>
                <Text style={styles.sectionTitle}>Quick Add</Text>
                <View style={styles.quickAddRow}>
                  <TextInput
                    autoCapitalize="sentences"
                    editable={!adding}
                    onChangeText={setQuickAddName}
                    onSubmitEditing={addQuickItem}
                    placeholder="Add an item"
                    placeholderTextColor="#8b817a"
                    returnKeyType="done"
                    style={styles.quickAddInput}
                    value={quickAddName}
                  />
                  <Pressable
                    accessibilityLabel="Add item to shopping list"
                    accessibilityRole="button"
                    disabled={!canMutate || adding || !quickAddName.trim()}
                    onPress={addQuickItem}
                    style={({ pressed }) => [
                      styles.addButton,
                      (!canMutate || adding || !quickAddName.trim()) && styles.disabled,
                      pressed && styles.pressed,
                    ]}>
                    {adding ? (
                      <ActivityIndicator color="#ffffff" size="small" />
                    ) : (
                      <MaterialIcons color="#ffffff" name="add" size={20} />
                    )}
                  </Pressable>
                </View>
              </View>

              {loading ? (
                <View style={styles.loadingPanel}>
                  <ActivityIndicator color="#526049" />
                  <Text style={styles.loadingText}>Loading shopping list</Text>
                </View>
              ) : null}

              {error ? (
                <View style={styles.errorPanel}>
                  <MaterialIcons color="#944931" name="error-outline" size={22} />
                  <View style={styles.errorCopy}>
                    <Text style={styles.errorTitle}>Could not update shopping list</Text>
                    <Text style={styles.errorText}>{error}</Text>
                  </View>
                </View>
              ) : null}

              <View style={styles.listTitleRow}>
                <View>
                  <Text style={styles.sectionTitle}>Items</Text>
                  <Text style={styles.sectionMeta}>{formatCount(items.length, 'item')}</Text>
                </View>
              </View>
            </View>
          }
          contentContainerStyle={styles.listContent}
          data={items}
          ItemSeparatorComponent={() => <View style={styles.separator} />}
          keyExtractor={(item) => item.id}
          keyboardShouldPersistTaps="handled"
          ListEmptyComponent={
            !loading && !error ? (
              <View style={styles.emptyPanel}>
                <MaterialIcons color="#526049" name="shopping-bag" size={26} />
                <Text style={styles.emptyTitle}>Nothing on the list</Text>
                <Text style={styles.emptyText}>Add groceries here or from a space item row.</Text>
              </View>
            ) : null
          }
          refreshControl={
            <RefreshControl
              onRefresh={() => loadShoppingList({ refresh: true })}
              refreshing={refreshing}
              tintColor="#526049"
            />
          }
          renderItem={({ item }) => (
            <ShoppingListRow
              deleting={deletingItemId === item.id}
              draft={editingDraft}
              editing={editingItemId === item.id}
              item={item}
              onCancelEdit={cancelEditing}
              onChangeDraft={setEditingDraft}
              onDelete={confirmDeleteItem}
              onEdit={startEditing}
              onSave={saveEditing}
              onToggle={toggleChecked}
              saving={savingItemId === item.id}
              toggling={togglingItemId === item.id}
            />
          )}
        />
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

function ShoppingListRow({
  deleting,
  draft,
  editing,
  item,
  onCancelEdit,
  onChangeDraft,
  onDelete,
  onEdit,
  onSave,
  onToggle,
  saving,
  toggling,
}: {
  deleting: boolean;
  draft: DraftValue;
  editing: boolean;
  item: ShoppingListItemView;
  onCancelEdit: () => void;
  onChangeDraft: (draft: DraftValue) => void;
  onDelete: (item: ShoppingListItemView) => void;
  onEdit: (item: ShoppingListItemView) => void;
  onSave: (item: ShoppingListItemView) => void;
  onToggle: (item: ShoppingListItemView) => void;
  saving: boolean;
  toggling: boolean;
}) {
  return (
    <View style={[styles.itemCard, item.checked && styles.itemCardChecked]}>
      <View style={styles.itemTopRow}>
        <Pressable
          accessibilityLabel={item.checked ? 'Mark item unchecked' : 'Mark item checked'}
          accessibilityRole="checkbox"
          accessibilityState={{ checked: item.checked, disabled: toggling }}
          disabled={toggling}
          onPress={() => onToggle(item)}
          style={({ pressed }) => [styles.checkButton, pressed && styles.pressed]}>
          {toggling ? (
            <ActivityIndicator color="#526049" size="small" />
          ) : (
            <MaterialIcons
              color={item.checked ? '#526049' : '#8b817a'}
              name={item.checked ? 'check-circle' : 'radio-button-unchecked'}
              size={26}
            />
          )}
        </Pressable>

        <View style={styles.itemContent}>
          {editing ? (
            <View style={styles.editFields}>
              <TextInput
                onChangeText={(name) => onChangeDraft({ ...draft, name })}
                placeholder="Name"
                placeholderTextColor="#8b817a"
                style={styles.editInput}
                value={draft.name}
              />
              <View style={styles.editGrid}>
                <TextInput
                  keyboardType="decimal-pad"
                  onChangeText={(quantity) => onChangeDraft({ ...draft, quantity })}
                  placeholder="Quantity"
                  placeholderTextColor="#8b817a"
                  style={styles.editInput}
                  value={draft.quantity}
                />
                <TextInput
                  keyboardType="decimal-pad"
                  onChangeText={(containerQuantity) => onChangeDraft({ ...draft, containerQuantity })}
                  placeholder="Container amount"
                  placeholderTextColor="#8b817a"
                  style={styles.editInput}
                  value={draft.containerQuantity}
                />
              </View>
              <View style={styles.unitSelector}>
                <Text style={styles.unitSelectorLabel}>Container type</Text>
                <View style={styles.unitOptions}>
                  {unitOptions.map((option) => {
                    const selected = draft.containerUnit === option.value;

                    return (
                      <Pressable
                        accessibilityRole="button"
                        accessibilityState={{ selected }}
                        key={option.value || 'none'}
                        onPress={() => onChangeDraft({ ...draft, containerUnit: option.value })}
                        style={({ pressed }) => [
                          styles.unitOption,
                          selected && styles.unitOptionSelected,
                          pressed && styles.pressed,
                        ]}>
                        <Text
                          style={[
                            styles.unitOptionText,
                            selected && styles.unitOptionTextSelected,
                          ]}>
                          {option.label}
                        </Text>
                      </Pressable>
                    );
                  })}
                </View>
              </View>
              <TextInput
                multiline
                onChangeText={(note) => onChangeDraft({ ...draft, note })}
                placeholder="Note"
                placeholderTextColor="#8b817a"
                style={[styles.editInput, styles.noteInput]}
                value={draft.note}
              />
            </View>
          ) : (
            <>
              <Text
                numberOfLines={1}
                style={[styles.itemName, item.checked && styles.itemNameChecked]}>
                {item.name}
              </Text>
              <Text style={styles.itemMeta}>{formatItemMeta(item)}</Text>
              {item.note ? <Text style={styles.itemNote}>{item.note}</Text> : null}
            </>
          )}
        </View>
      </View>

      <View style={styles.itemActions}>
        {editing ? (
          <>
            <Pressable
              accessibilityRole="button"
              disabled={saving}
              onPress={onCancelEdit}
              style={({ pressed }) => [styles.secondaryButton, pressed && styles.pressed]}>
              <Text style={styles.secondaryButtonText}>Cancel</Text>
            </Pressable>
            <Pressable
              accessibilityRole="button"
              disabled={saving}
              onPress={() => onSave(item)}
              style={({ pressed }) => [
                styles.primaryButton,
                saving && styles.disabled,
                pressed && styles.pressed,
              ]}>
              {saving ? (
                <ActivityIndicator color="#ffffff" size="small" />
              ) : (
                <Text style={styles.primaryButtonText}>Save</Text>
              )}
            </Pressable>
          </>
        ) : (
          <>
            <Pressable
              accessibilityLabel="Edit shopping list item"
              accessibilityRole="button"
              onPress={() => onEdit(item)}
              style={({ pressed }) => [styles.iconButton, pressed && styles.pressed]}>
              <MaterialIcons color="#526049" name="edit" size={18} />
            </Pressable>
            <Pressable
              accessibilityLabel="Delete shopping list item"
              accessibilityRole="button"
              disabled={deleting}
              onPress={() => onDelete(item)}
              style={({ pressed }) => [
                styles.iconButton,
                styles.deleteButton,
                deleting && styles.disabled,
                pressed && styles.pressed,
              ]}>
              {deleting ? (
                <ActivityIndicator color="#944931" size="small" />
              ) : (
                <MaterialIcons color="#944931" name="delete-outline" size={18} />
              )}
            </Pressable>
          </>
        )}
      </View>
    </View>
  );
}

function SummaryCard({
  icon,
  label,
  value,
}: {
  icon: React.ComponentProps<typeof MaterialIcons>['name'];
  label: string;
  value: number;
}) {
  return (
    <View style={styles.summaryCard}>
      <MaterialIcons color="#526049" name={icon} size={22} />
      <Text style={styles.summaryValue}>{value}</Text>
      <Text style={styles.summaryLabel}>{label}</Text>
    </View>
  );
}

function firstParam(value?: string | string[]) {
  return Array.isArray(value) ? value[0] : value;
}

function emptyDraft(): DraftValue {
  return {
    containerQuantity: '',
    containerUnit: '',
    name: '',
    note: '',
    quantity: '',
  };
}

function replaceItem(items: ShoppingListItemView[], nextItem: ShoppingListItemView) {
  return items.map((item) => (item.id === nextItem.id ? nextItem : item));
}

function parseOptionalQuantity(value: string) {
  const trimmed = value.trim();

  if (!trimmed) {
    return null;
  }

  const parsed = Number(trimmed.replace(',', '.'));
  return Number.isFinite(parsed) && parsed > 0 ? parsed : undefined;
}

function formatItemMeta(item: ShoppingListItemView) {
  const quantity = item.quantity === null ? null : item.quantity.toLocaleString();
  const containerQuantity = item.containerQuantity === null
    ? null
    : item.containerQuantity.toLocaleString();
  const containerUnit = item.containerUnit?.trim() || null;

  if (quantity && containerQuantity && containerUnit) {
    return `${quantity} x ${containerQuantity} ${containerUnit}`;
  }

  if (quantity && containerQuantity) {
    return `${quantity} x ${containerQuantity}`;
  }

  if (quantity) {
    return `${quantity} containers`;
  }

  if (item.itemId) {
    return 'Linked inventory item';
  }

  return 'No quantity set';
}

function formatCount(value: number, noun: string) {
  return `${value} ${noun}${value === 1 ? '' : 's'}`;
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
      return 'This shopping list was not found.';
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
    flex: 1,
    backgroundColor: '#fff8f3',
  },
  screen: {
    flex: 1,
  },
  listContent: {
    paddingBottom: 32,
    paddingHorizontal: 20,
  },
  headerContent: {
    gap: 20,
    paddingTop: 20,
  },
  summaryGrid: {
    flexDirection: 'row',
    gap: 10,
  },
  summaryCard: {
    backgroundColor: '#ffffff',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    flex: 1,
    gap: 6,
    minHeight: 104,
    padding: 16,
  },
  summaryValue: {
    color: '#1e1b18',
    fontSize: 28,
    fontWeight: '800',
    letterSpacing: 0,
  },
  summaryLabel: {
    color: '#444841',
    fontSize: 13,
    fontWeight: '700',
  },
  quickAddPanel: {
    backgroundColor: '#ffffff',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    gap: 12,
    padding: 14,
  },
  sectionTitle: {
    color: '#1e1b18',
    fontSize: 20,
    fontWeight: '800',
    letterSpacing: 0,
  },
  sectionMeta: {
    color: '#757870',
    fontSize: 13,
    fontWeight: '700',
    marginTop: 3,
  },
  quickAddRow: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 10,
  },
  quickAddInput: {
    backgroundColor: '#fff8f3',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    color: '#1e1b18',
    flex: 1,
    fontSize: 16,
    minHeight: 46,
    paddingHorizontal: 12,
  },
  addButton: {
    alignItems: 'center',
    backgroundColor: '#526049',
    borderRadius: 8,
    height: 46,
    justifyContent: 'center',
    width: 46,
  },
  listTitleRow: {
    marginTop: 2,
  },
  separator: {
    height: 12,
  },
  itemCard: {
    backgroundColor: '#ffffff',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    gap: 12,
    padding: 14,
  },
  itemCardChecked: {
    backgroundColor: '#f8f4ef',
  },
  itemTopRow: {
    alignItems: 'flex-start',
    flexDirection: 'row',
    gap: 12,
  },
  checkButton: {
    alignItems: 'center',
    height: 34,
    justifyContent: 'center',
    width: 34,
  },
  itemContent: {
    flex: 1,
    gap: 5,
    minWidth: 0,
  },
  itemName: {
    color: '#1e1b18',
    fontSize: 17,
    fontWeight: '800',
    letterSpacing: 0,
  },
  itemNameChecked: {
    color: '#757870',
    textDecorationLine: 'line-through',
  },
  itemMeta: {
    color: '#444841',
    fontSize: 13,
    fontWeight: '700',
  },
  itemNote: {
    color: '#757870',
    fontSize: 13,
    lineHeight: 18,
  },
  itemActions: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 8,
    justifyContent: 'flex-end',
  },
  iconButton: {
    alignItems: 'center',
    backgroundColor: '#faf2ed',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    height: 38,
    justifyContent: 'center',
    width: 38,
  },
  deleteButton: {
    backgroundColor: '#fff5f1',
    borderColor: '#ffb59e',
  },
  editFields: {
    gap: 8,
  },
  editGrid: {
    flexDirection: 'row',
    gap: 8,
  },
  editInput: {
    backgroundColor: '#fff8f3',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    color: '#1e1b18',
    flex: 1,
    fontSize: 15,
    minHeight: 42,
    paddingHorizontal: 11,
  },
  unitSelector: {
    gap: 8,
  },
  unitSelectorLabel: {
    color: '#444841',
    fontSize: 13,
    fontWeight: '800',
  },
  unitOptions: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  unitOption: {
    alignItems: 'center',
    backgroundColor: '#faf2ed',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    minHeight: 38,
    paddingHorizontal: 12,
    justifyContent: 'center',
  },
  unitOptionSelected: {
    backgroundColor: '#526049',
    borderColor: '#526049',
  },
  unitOptionText: {
    color: '#526049',
    fontSize: 13,
    fontWeight: '800',
  },
  unitOptionTextSelected: {
    color: '#ffffff',
  },
  noteInput: {
    minHeight: 74,
    paddingTop: 10,
    textAlignVertical: 'top',
  },
  primaryButton: {
    alignItems: 'center',
    backgroundColor: '#526049',
    borderRadius: 8,
    justifyContent: 'center',
    minHeight: 40,
    minWidth: 88,
    paddingHorizontal: 14,
  },
  primaryButtonText: {
    color: '#ffffff',
    fontSize: 14,
    fontWeight: '800',
  },
  secondaryButton: {
    alignItems: 'center',
    backgroundColor: '#faf2ed',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    justifyContent: 'center',
    minHeight: 40,
    minWidth: 88,
    paddingHorizontal: 14,
  },
  secondaryButtonText: {
    color: '#526049',
    fontSize: 14,
    fontWeight: '800',
  },
  emptyPanel: {
    alignItems: 'center',
    gap: 8,
    paddingHorizontal: 20,
    paddingVertical: 36,
  },
  emptyTitle: {
    color: '#1e1b18',
    fontSize: 17,
    fontWeight: '800',
  },
  emptyText: {
    color: '#757870',
    fontSize: 14,
    lineHeight: 20,
    textAlign: 'center',
  },
  loadingPanel: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 10,
    paddingVertical: 4,
  },
  loadingText: {
    color: '#444841',
    fontSize: 14,
    fontWeight: '600',
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
    gap: 8,
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
  disabled: {
    opacity: 0.5,
  },
  pressed: {
    opacity: 0.78,
  },
});
