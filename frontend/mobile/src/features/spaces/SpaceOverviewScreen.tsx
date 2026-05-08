import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { router, useLocalSearchParams } from 'expo-router';
import { type ComponentProps, useCallback, useEffect, useMemo, useState } from 'react';
import {
  ActivityIndicator,
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

import { ApiError } from '@/core/http/apiClient';
import { useAuthSession } from '@/features/auth/authSession';
import { getHousehold, type HouseholdView } from '@/features/households/api';
import { getItems, type ItemView } from '@/features/items/api';
import {
  createSpace,
  getSpace,
  getSpaces,
  type SpacePage,
  type SpaceView,
} from '@/features/spaces/api';

type OverviewTab = 'subSpaces' | 'items';
type OverviewListItem = SpaceView | ItemView;

export default function SpaceOverviewScreen() {
  const { householdId, parentId } = useLocalSearchParams<{
    householdId?: string | string[];
    parentId?: string | string[];
  }>();
  const { accessToken } = useAuthSession();
  const resolvedHouseholdId = firstParam(householdId);
  const resolvedParentId = firstParam(parentId);
  const [household, setHousehold] = useState<HouseholdView | null>(null);
  const [parentSpace, setParentSpace] = useState<SpaceView | null>(null);
  const [page, setPage] = useState<SpacePage | null>(null);
  const [items, setItems] = useState<ItemView[]>([]);
  const [activeTab, setActiveTab] = useState<OverviewTab>('subSpaces');
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [refreshing, setRefreshing] = useState(false);
  const [creating, setCreating] = useState(false);
  const [showCreateSpace, setShowCreateSpace] = useState(false);
  const trimmedName = name.trim();
  const trimmedDescription = description.trim();
  const spaces = useMemo(() => page?.spaces ?? [], [page?.spaces]);
  const currentData = useMemo<OverviewListItem[]>(
    () => (activeTab === 'subSpaces' ? spaces : items),
    [activeTab, items, spaces],
  );
  const canCreate = Boolean(accessToken && resolvedHouseholdId && trimmedName && !creating);
  const screenTitle = parentSpace?.name ?? household?.name ?? 'Space Overview';

  const summary = useMemo(() => {
    const itemCount = resolvedParentId
      ? items.length
      : spaces.reduce((total, space) => total + (space.items?.count ?? 0), 0);

    return {
      itemCount,
      spaceCount: page?.totalCount ?? spaces.length,
    };
  }, [items.length, page?.totalCount, resolvedParentId, spaces]);

  const loadOverview = useCallback(
    async ({ refresh = false } = {}) => {
      if (!accessToken || !resolvedHouseholdId) {
        setError('Sign in and choose a household to view spaces.');
        setHousehold(null);
        setParentSpace(null);
        setPage(null);
        setItems([]);
        return;
      }

      if (refresh) {
        setRefreshing(true);
      } else {
        setLoading(true);
      }

      setError(null);

      try {
        const [nextHousehold, nextParentSpace, nextPage, nextItems] = await Promise.all([
          getHousehold(resolvedHouseholdId, { accessToken }),
          resolvedParentId
            ? getSpace(resolvedHouseholdId, resolvedParentId, { accessToken })
            : Promise.resolve(null),
          getSpaces(
            resolvedHouseholdId,
            {
              includeChildSpaceCount: true,
              includeItemCount: true,
              pageSize: 100,
              parentId: resolvedParentId,
            },
            { accessToken },
          ),
          resolvedParentId
            ? getItems(resolvedHouseholdId, resolvedParentId, { accessToken })
            : Promise.resolve([]),
        ]);

        setHousehold(nextHousehold);
        setParentSpace(nextParentSpace);
        setPage(nextPage);
        setItems(nextItems);
      } catch (exception) {
        setError(getUserFacingError(exception));
      } finally {
        setLoading(false);
        setRefreshing(false);
      }
    },
    [accessToken, resolvedHouseholdId, resolvedParentId],
  );

  useEffect(() => {
    loadOverview();
  }, [loadOverview]);

  const submitSpace = useCallback(async () => {
    if (!accessToken || !resolvedHouseholdId || !trimmedName) {
      return;
    }

    setCreating(true);
    setError(null);

    try {
      const space = await createSpace(
        resolvedHouseholdId,
        {
          description: trimmedDescription || null,
          name: trimmedName,
          parentId: resolvedParentId,
        },
        { accessToken },
      );

      setPage((currentPage) => {
        if (!currentPage) {
          return {
            pageNumber: 1,
            pageSize: 100,
            spaces: [space],
            totalCount: 1,
          };
        }

        return {
          ...currentPage,
          spaces: [space, ...currentPage.spaces],
          totalCount: currentPage.totalCount + 1,
        };
      });
      setName('');
      setDescription('');
      setShowCreateSpace(false);
    } catch (exception) {
      setError(getUserFacingError(exception));
    } finally {
      setCreating(false);
    }
  }, [
    accessToken,
    resolvedHouseholdId,
    resolvedParentId,
    trimmedDescription,
    trimmedName,
  ]);

  const subtitle = parentSpace
    ? parentSpace.description || `Inside ${household?.name ?? 'household'}`
    : 'Top-level spaces in this household';

  return (
    <SafeAreaView style={styles.safeArea}>
      <KeyboardAvoidingView
        behavior={Platform.select({ ios: 'padding', default: undefined })}
        style={styles.screen}>
        <FlatList<OverviewListItem>
          ListHeaderComponent={
            <View style={styles.content}>
              <View style={styles.header}>
                <View style={styles.headerActions}>
                  <Pressable
                    accessibilityLabel="Go back"
                    accessibilityRole="button"
                    onPress={() => router.back()}
                    style={({ pressed }) => [styles.iconButton, pressed && styles.pressed]}>
                    <MaterialIcons color="#526049" name="arrow-back" size={22} />
                  </Pressable>
                  <Pressable
                    accessibilityLabel="Refresh spaces"
                    accessibilityRole="button"
                    disabled={loading || refreshing || !accessToken}
                    onPress={() => loadOverview({ refresh: true })}
                    style={({ pressed }) => [
                      styles.iconButton,
                      pressed && styles.pressed,
                      (!accessToken || loading || refreshing) && styles.disabledButton,
                    ]}>
                    <MaterialIcons color="#526049" name="refresh" size={22} />
                  </Pressable>
                </View>

                <View style={styles.titleBlock}>
                  <Text style={styles.eyebrow}>
                    {parentSpace ? 'Space View' : 'Household Spaces'}
                  </Text>
                  <Text style={styles.title}>{screenTitle}</Text>
                  <Text style={styles.body}>{subtitle}</Text>
                </View>
              </View>

              <View style={styles.summaryGrid}>
                <SummaryMetric label="Spaces" value={summary.spaceCount} />
                <SummaryMetric label="Items" value={summary.itemCount} />
              </View>

              <View style={styles.tabs}>
                <TabButton
                  active={activeTab === 'subSpaces'}
                  label="Sub-spaces"
                  onPress={() => setActiveTab('subSpaces')}
                />
                <TabButton
                  active={activeTab === 'items'}
                  label="Items"
                  onPress={() => setActiveTab('items')}
                />
              </View>

              <View style={styles.listSection}>
                <View style={styles.sectionHeader}>
                  <View>
                    <Text style={styles.sectionTitle}>
                      {activeTab === 'subSpaces' ? 'Sub-spaces' : 'Items'}
                    </Text>
                    <Text style={styles.sectionMeta}>
                      {activeTab === 'subSpaces'
                        ? formatCount(summary.spaceCount, 'space')
                        : formatCount(items.length, 'item')}
                    </Text>
                  </View>
                  {activeTab === 'subSpaces' && (
                    <Pressable
                      accessibilityLabel={
                        showCreateSpace ? 'Hide add sub-space form' : 'Show add sub-space form'
                      }
                      accessibilityRole="button"
                      onPress={() => setShowCreateSpace((visible) => !visible)}
                      style={({ pressed }) => [styles.ghostActionButton, pressed && styles.pressed]}>
                      <MaterialIcons
                        color="#526049"
                        name={showCreateSpace ? 'close' : 'add'}
                        size={18}
                      />
                      <Text style={styles.ghostActionText}>
                        {showCreateSpace ? 'Close' : 'Add Sub-space'}
                      </Text>
                    </Pressable>
                  )}
                </View>

                {activeTab === 'subSpaces' && showCreateSpace && (
                  <View style={styles.formPanel}>
                    <View style={styles.formHeader}>
                      <MaterialIcons color="#526049" name="create-new-folder" size={22} />
                      <Text style={styles.formTitle}>Add Sub-space</Text>
                    </View>

                    <View style={styles.field}>
                      <Text style={styles.label}>Name</Text>
                      <TextInput
                        autoCapitalize="words"
                        onChangeText={setName}
                        onSubmitEditing={submitSpace}
                        placeholder="Pantry"
                        placeholderTextColor="#8c8a81"
                        returnKeyType="next"
                        style={styles.input}
                        value={name}
                      />
                    </View>

                    <View style={styles.field}>
                      <Text style={styles.label}>Description</Text>
                      <TextInput
                        multiline
                        onChangeText={setDescription}
                        placeholder="Dry goods and weekly essentials"
                        placeholderTextColor="#8c8a81"
                        style={[styles.input, styles.textArea]}
                        value={description}
                      />
                    </View>

                    <Pressable
                      accessibilityRole="button"
                      disabled={!canCreate}
                      onPress={submitSpace}
                      style={({ pressed }) => [
                        styles.primaryButton,
                        pressed && styles.primaryButtonPressed,
                        !canCreate && styles.primaryButtonDisabled,
                      ]}>
                      {creating ? (
                        <ActivityIndicator color="#ffffff" />
                      ) : (
                        <>
                          <MaterialIcons color="#ffffff" name="add" size={20} />
                          <Text style={styles.primaryButtonText}>Create sub-space</Text>
                        </>
                      )}
                    </Pressable>
                  </View>
                )}
              </View>

              {error && (
                <View style={styles.errorPanel}>
                  <MaterialIcons color="#944931" name="error-outline" size={22} />
                  <View style={styles.errorCopy}>
                    <Text style={styles.errorTitle}>Could not load spaces</Text>
                    <Text style={styles.errorText}>{error}</Text>
                  </View>
                </View>
              )}

              {loading && (
                <View style={styles.loadingPanel}>
                  <ActivityIndicator color="#526049" />
                  <Text style={styles.loadingText}>Loading spaces</Text>
                </View>
              )}
            </View>
          }
          ListHeaderComponentStyle={styles.listHeader}
          contentContainerStyle={styles.listContent}
          data={currentData}
          ItemSeparatorComponent={() => <View style={styles.separator} />}
          keyExtractor={(item) => item.id}
          keyboardShouldPersistTaps="handled"
          ListEmptyComponent={
            !loading && !error ? (
              activeTab === 'subSpaces' ? (
                <EmptySpaces onCreate={() => setShowCreateSpace(true)} />
              ) : (
                <EmptyItems hasSelectedSpace={Boolean(resolvedParentId)} />
              )
            ) : null
          }
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={() => loadOverview({ refresh: true })}
              tintColor="#526049"
            />
          }
          renderItem={({ item }) =>
            resolvedHouseholdId && activeTab === 'subSpaces' ? (
              <SpaceCard householdId={resolvedHouseholdId} space={item as SpaceView} />
            ) : activeTab === 'items' ? (
              <ItemCard item={item as ItemView} />
            ) : null
          }
        />
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

function TabButton({
  active,
  label,
  onPress,
}: {
  active: boolean;
  label: string;
  onPress: () => void;
}) {
  return (
    <Pressable
      accessibilityRole="tab"
      accessibilityState={{ selected: active }}
      onPress={onPress}
      style={({ pressed }) => [
        styles.tabButton,
        active && styles.tabButtonActive,
        pressed && styles.pressed,
      ]}>
      <Text style={[styles.tabButtonText, active && styles.tabButtonTextActive]}>{label}</Text>
    </Pressable>
  );
}

function SpaceCard({ householdId, space }: { householdId: string; space: SpaceView }) {
  const openSpace = useCallback(() => {
    router.push({
      pathname: '/households/[householdId]/spaces',
      params: { householdId, parentId: space.id },
    });
  }, [householdId, space.id]);

  const itemCount = space.items?.count ?? 0;
  const childCount = space.childSpaces?.count ?? 0;

  return (
    <Pressable
      accessibilityRole="button"
      onPress={openSpace}
      style={({ pressed }) => [styles.spaceCard, pressed && styles.cardPressed]}>
      <View style={styles.spaceIcon}>
        <MaterialIcons color="#526049" name="inventory-2" size={24} />
      </View>

      <View style={styles.spaceContent}>
        <Text numberOfLines={1} style={styles.spaceName}>
          {space.name}
        </Text>
        {space.description ? (
          <Text numberOfLines={2} style={styles.spaceDescription}>
            {space.description}
          </Text>
        ) : null}
        <View style={styles.spaceMetaRow}>
          <SpaceMeta icon="category" label={formatCount(childCount, 'space')} />
          <SpaceMeta icon="kitchen" label={formatCount(itemCount, 'item')} />
        </View>
      </View>

      <MaterialIcons color="#757870" name="chevron-right" size={24} />
    </Pressable>
  );
}

function ItemCard({ item }: { item: ItemView }) {
  return (
    <Pressable
      accessibilityRole="button"
      style={({ pressed }) => [styles.itemCard, pressed && styles.cardPressed]}>
      <View style={styles.itemIcon}>
        <MaterialIcons color="#944931" name="kitchen" size={24} />
      </View>

      <View style={styles.spaceContent}>
        <Text numberOfLines={1} style={styles.spaceName}>
          {item.name}
        </Text>
        <Text style={styles.spaceDescription}>
          {item.category || 'Uncategorized'} - {formatQuantity(item.totalQuantity)}
        </Text>
        <View style={styles.spaceMetaRow}>
          <SpaceMeta icon="inventory" label={formatCount(item.entries.length, 'entry')} />
          {item.barcode ? <SpaceMeta icon="qr-code-2" label={item.barcode} /> : null}
        </View>
      </View>

      <MaterialIcons color="#757870" name="chevron-right" size={24} />
    </Pressable>
  );
}

function SummaryMetric({ label, value }: { label: string; value: number }) {
  return (
    <View style={styles.summaryMetric}>
      <Text style={styles.summaryValue}>{value}</Text>
      <Text style={styles.summaryLabel}>{label}</Text>
    </View>
  );
}

function SpaceMeta({
  icon,
  label,
}: {
  icon: ComponentProps<typeof MaterialIcons>['name'];
  label: string;
}) {
  return (
    <View style={styles.spaceMeta}>
      <MaterialIcons color="#757870" name={icon} size={15} />
      <Text style={styles.spaceMetaText}>{label}</Text>
    </View>
  );
}

function EmptySpaces({ onCreate }: { onCreate: () => void }) {
  return (
    <View style={styles.emptyPanel}>
      <View style={styles.emptyIcon}>
        <MaterialIcons color="#526049" name="inventory-2" size={28} />
      </View>
      <Text style={styles.emptyTitle}>No sub-spaces here yet</Text>
      <Text style={styles.emptyText}>Add a sub-space to organize this part of the home.</Text>
      <Pressable
        accessibilityRole="button"
        onPress={onCreate}
        style={({ pressed }) => [styles.emptyButton, pressed && styles.pressed]}>
        <Text style={styles.emptyButtonText}>Create sub-space</Text>
      </Pressable>
    </View>
  );
}

function EmptyItems({ hasSelectedSpace }: { hasSelectedSpace: boolean }) {
  return (
    <View style={styles.emptyPanel}>
      <View style={styles.emptyIcon}>
        <MaterialIcons color="#944931" name="kitchen" size={28} />
      </View>
      <Text style={styles.emptyTitle}>{hasSelectedSpace ? 'No items here yet' : 'Open a space first'}</Text>
      <Text style={styles.emptyText}>
        {hasSelectedSpace
          ? 'Items added to this space will appear here.'
          : 'Household-level item browsing needs a selected space.'}
      </Text>
    </View>
  );
}

function firstParam(value?: string | string[]) {
  return Array.isArray(value) ? value[0] : value;
}

function formatCount(value: number, noun: string) {
  return `${value} ${noun}${value === 1 ? '' : 's'}`;
}

function formatQuantity(value: number) {
  return `${value.toLocaleString()} total`;
}

function getUserFacingError(exception: unknown) {
  if (exception instanceof ApiError) {
    if (exception.status === 401) {
      return 'Your session is missing or expired. Sign in again.';
    }

    if (exception.status === 404) {
      return 'This household or space was not found.';
    }

    return exception.message;
  }

  return 'Check that the backend is running at the configured API URL.';
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: '#fff8f3',
  },
  screen: {
    flex: 1,
    backgroundColor: '#fff8f3',
  },
  listContent: {
    paddingBottom: 32,
    paddingHorizontal: 20,
  },
  listHeader: {
    marginBottom: 12,
  },
  content: {
    gap: 24,
    paddingTop: 20,
  },
  header: {
    gap: 18,
  },
  headerActions: {
    alignItems: 'center',
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  titleBlock: {
    gap: 8,
  },
  eyebrow: {
    color: '#526049',
    fontSize: 13,
    fontWeight: '700',
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  title: {
    color: '#1e1b18',
    fontSize: 32,
    fontWeight: '800',
    letterSpacing: 0,
    lineHeight: 39,
  },
  body: {
    color: '#444841',
    fontSize: 16,
    lineHeight: 24,
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
  disabledButton: {
    opacity: 0.5,
  },
  pressed: {
    opacity: 0.78,
  },
  summaryGrid: {
    flexDirection: 'row',
    gap: 10,
  },
  summaryMetric: {
    backgroundColor: '#faf2ed',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    flex: 1,
    gap: 4,
    minHeight: 78,
    padding: 14,
  },
  summaryValue: {
    color: '#1e1b18',
    fontSize: 24,
    fontWeight: '800',
    letterSpacing: 0,
  },
  summaryLabel: {
    color: '#444841',
    fontSize: 13,
    fontWeight: '700',
  },
  tabs: {
    backgroundColor: '#faf2ed',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    gap: 4,
    padding: 4,
  },
  tabButton: {
    alignItems: 'center',
    borderRadius: 8,
    flex: 1,
    justifyContent: 'center',
    minHeight: 42,
    paddingHorizontal: 12,
  },
  tabButtonActive: {
    backgroundColor: '#526049',
  },
  tabButtonText: {
    color: '#444841',
    fontSize: 14,
    fontWeight: '800',
    letterSpacing: 0,
  },
  tabButtonTextActive: {
    color: '#ffffff',
  },
  formPanel: {
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
  formHeader: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 8,
  },
  formTitle: {
    color: '#1e1b18',
    fontSize: 18,
    fontWeight: '700',
    letterSpacing: 0,
  },
  field: {
    gap: 8,
  },
  label: {
    color: '#444841',
    fontSize: 13,
    fontWeight: '700',
    letterSpacing: 0,
  },
  input: {
    backgroundColor: '#faf2ed',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    color: '#1e1b18',
    fontSize: 16,
    minHeight: 52,
    paddingHorizontal: 14,
  },
  textArea: {
    minHeight: 86,
    paddingTop: 14,
    textAlignVertical: 'top',
  },
  primaryButton: {
    alignItems: 'center',
    backgroundColor: '#526049',
    borderRadius: 8,
    flexDirection: 'row',
    gap: 8,
    justifyContent: 'center',
    minHeight: 52,
    paddingHorizontal: 16,
  },
  primaryButtonDisabled: {
    backgroundColor: '#9ca58f',
  },
  primaryButtonPressed: {
    opacity: 0.86,
  },
  primaryButtonText: {
    color: '#ffffff',
    fontSize: 16,
    fontWeight: '700',
    letterSpacing: 0,
  },
  listSection: {
    gap: 12,
  },
  sectionHeader: {
    alignItems: 'center',
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginTop: 2,
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
  ghostActionButton: {
    alignItems: 'center',
    backgroundColor: '#faf2ed',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    gap: 6,
    justifyContent: 'center',
    minHeight: 42,
    paddingHorizontal: 12,
  },
  ghostActionText: {
    color: '#526049',
    fontSize: 13,
    fontWeight: '800',
    letterSpacing: 0,
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
  separator: {
    height: 12,
  },
  spaceCard: {
    alignItems: 'center',
    backgroundColor: '#ffffff',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    gap: 12,
    minHeight: 92,
    padding: 14,
  },
  itemCard: {
    alignItems: 'center',
    backgroundColor: '#ffffff',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    gap: 12,
    minHeight: 92,
    padding: 14,
  },
  cardPressed: {
    backgroundColor: '#faf2ed',
  },
  spaceIcon: {
    alignItems: 'center',
    backgroundColor: '#d8e8cb',
    borderRadius: 8,
    height: 48,
    justifyContent: 'center',
    width: 48,
  },
  itemIcon: {
    alignItems: 'center',
    backgroundColor: '#ffdbd0',
    borderRadius: 8,
    height: 48,
    justifyContent: 'center',
    width: 48,
  },
  spaceContent: {
    flex: 1,
    gap: 6,
    minWidth: 0,
  },
  spaceName: {
    color: '#1e1b18',
    fontSize: 17,
    fontWeight: '800',
    letterSpacing: 0,
  },
  spaceDescription: {
    color: '#444841',
    fontSize: 13,
    lineHeight: 18,
  },
  spaceMetaRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  spaceMeta: {
    alignItems: 'center',
    backgroundColor: '#faf2ed',
    borderRadius: 8,
    flexDirection: 'row',
    gap: 4,
    minHeight: 26,
    paddingHorizontal: 8,
  },
  spaceMetaText: {
    color: '#444841',
    fontSize: 12,
    fontWeight: '700',
  },
  emptyPanel: {
    alignItems: 'center',
    backgroundColor: '#faf2ed',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    gap: 8,
    padding: 24,
  },
  emptyIcon: {
    alignItems: 'center',
    backgroundColor: '#d8e8cb',
    borderRadius: 8,
    height: 54,
    justifyContent: 'center',
    marginBottom: 4,
    width: 54,
  },
  emptyTitle: {
    color: '#1e1b18',
    fontSize: 18,
    fontWeight: '800',
  },
  emptyText: {
    color: '#444841',
    fontSize: 14,
    lineHeight: 20,
    textAlign: 'center',
  },
  emptyButton: {
    alignItems: 'center',
    backgroundColor: '#526049',
    borderRadius: 8,
    justifyContent: 'center',
    marginTop: 8,
    minHeight: 42,
    paddingHorizontal: 16,
  },
  emptyButtonText: {
    color: '#ffffff',
    fontSize: 14,
    fontWeight: '800',
  },
});
