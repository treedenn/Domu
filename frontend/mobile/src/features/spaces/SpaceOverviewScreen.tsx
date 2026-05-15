import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { useFocusEffect } from '@react-navigation/native';
import { router, useLocalSearchParams } from 'expo-router';
import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { TimeoutError } from '@/core/async/timeout';
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
import { AddSubSpaceForm } from '@/features/spaces/components/AddSubSpaceForm';
import { OverviewTabs, type OverviewTab } from '@/features/spaces/components/OverviewTabs';
import {
  SpaceOverviewList,
  type OverviewListItem,
} from '@/features/spaces/components/SpaceOverviewList';
import { SpaceOverviewSummary } from '@/features/spaces/components/SpaceOverviewSummary';
import { AppTopBar, type AppTopBarAction } from '@/ui/AppTopBar';

const addItemRoute = '/households/[householdId]/items/add' as never;
const itemDetailsRoute = '/households/[householdId]/items/[itemId]' as never;
const scannerRoute = '/households/[householdId]/items/scanner' as never;

export default function SpaceOverviewScreen() {
  const { householdId, parentId, tab } = useLocalSearchParams<{
    householdId?: string | string[];
    parentId?: string | string[];
    tab?: string | string[];
  }>();
  const { accessToken, clearTokenResponse } = useAuthSession();
  const resolvedHouseholdId = firstParam(householdId);
  const resolvedParentId = firstParam(parentId);
  const requestedTab = parseOverviewTab(firstParam(tab));
  const [household, setHousehold] = useState<HouseholdView | null>(null);
  const [parentSpace, setParentSpace] = useState<SpaceView | null>(null);
  const [page, setPage] = useState<SpacePage | null>(null);
  const [items, setItems] = useState<ItemView[]>([]);
  const [activeTab, setActiveTab] = useState<OverviewTab>(requestedTab ?? 'subSpaces');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [refreshing, setRefreshing] = useState(false);
  const [creating, setCreating] = useState(false);
  const [showCreateSpace, setShowCreateSpace] = useState(false);
  const [formResetKey, setFormResetKey] = useState(0);
  const spaces = useMemo(() => page?.spaces ?? [], [page?.spaces]);
  const currentData = useMemo<OverviewListItem[]>(
    () => (activeTab === 'subSpaces' ? spaces : items),
    [activeTab, items, spaces],
  );
  const screenTitle = parentSpace?.name ?? household?.name ?? 'Space Overview';

  const returnToSignIn = useCallback(async () => {
    await clearTokenResponse();
    router.replace('/');
  }, [clearTokenResponse]);

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
    [accessToken, resolvedHouseholdId, resolvedParentId, returnToSignIn],
  );

  useFocusEffect(
    useCallback(() => {
      loadOverview();
    }, [loadOverview]),
  );

  useEffect(() => {
    if (requestedTab) {
      setActiveTab(requestedTab);
    }
  }, [requestedTab]);

  const submitSpace = useCallback(async (name: string, description: string) => {
    const trimmedName = name.trim();
    const trimmedDescription = description.trim();

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
      setFormResetKey((key) => key + 1);
      setShowCreateSpace(false);
    } catch (exception) {
      if (isExpiredSessionError(exception)) {
        await returnToSignIn();
        return;
      }

      setError(getUserFacingError(exception));
    } finally {
      setCreating(false);
    }
  }, [
    accessToken,
    resolvedHouseholdId,
    resolvedParentId,
    returnToSignIn,
  ]);

  const subtitle = parentSpace
    ? parentSpace.description || `Inside ${household?.name ?? 'household'}`
    : 'Top-level spaces in this household';

  const openAddItem = useCallback(() => {
    if (!resolvedHouseholdId || !resolvedParentId) {
      return;
    }

    router.push({
      pathname: addItemRoute,
      params: { householdId: resolvedHouseholdId, spaceId: resolvedParentId },
    });
  }, [resolvedHouseholdId, resolvedParentId]);

  const openScanner = useCallback(() => {
    if (!resolvedHouseholdId || !resolvedParentId) {
      return;
    }

    router.push({
      pathname: scannerRoute,
      params: { householdId: resolvedHouseholdId, spaceId: resolvedParentId },
    });
  }, [resolvedHouseholdId, resolvedParentId]);

  const openSpace = useCallback(
    (space: SpaceView) => {
      if (!resolvedHouseholdId) {
        return;
      }

      router.push({
        pathname: '/households/[householdId]/spaces',
        params: { householdId: resolvedHouseholdId, parentId: space.id },
      });
    },
    [resolvedHouseholdId],
  );

  const openItem = useCallback(
    (item: ItemView) => {
      if (!resolvedHouseholdId || !resolvedParentId) {
        return;
      }

      router.push({
        pathname: itemDetailsRoute,
        params: { householdId: resolvedHouseholdId, itemId: item.id, spaceId: resolvedParentId },
      });
    },
    [resolvedHouseholdId, resolvedParentId],
  );

  const topBarActions = useMemo<AppTopBarAction[]>(
    () => [
      {
        disabled: loading || refreshing || !accessToken,
        icon: 'refresh',
        label: 'Refresh',
        onPress: () => loadOverview({ refresh: true }),
      },
    ],
    [accessToken, loadOverview, loading, refreshing],
  );

  return (
    <SafeAreaView style={styles.safeArea}>
      <KeyboardAvoidingView
        behavior={Platform.select({ ios: 'padding', default: undefined })}
        style={styles.screen}>
        <SpaceOverviewList
          activeTab={activeTab}
          data={currentData}
          error={error}
          hasSelectedSpace={Boolean(resolvedParentId)}
          listHeader={
            <View style={styles.content}>
              <View style={styles.header}>
                <AppTopBar
                  actions={topBarActions}
                  onBack={() => router.back()}
                  subtitle={parentSpace ? 'Space View' : 'Household Spaces'}
                  title={screenTitle}
                />

                <View style={styles.titleBlock}>
                  <Text style={styles.body}>{subtitle}</Text>
                </View>
              </View>

              <SpaceOverviewSummary
                itemCount={summary.itemCount}
                spaceCount={summary.spaceCount}
              />

              <OverviewTabs activeTab={activeTab} onChange={setActiveTab} />

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
                  {activeTab === 'items' && resolvedParentId && (
                    <View style={styles.itemActions}>
                      <Pressable
                        accessibilityLabel="Add item manually"
                        accessibilityRole="button"
                        onPress={openAddItem}
                        style={({ pressed }) => [
                          styles.iconActionButton,
                          pressed && styles.pressed,
                        ]}>
                        <MaterialIcons color="#526049" name="edit-note" size={18} />
                      </Pressable>
                      <Pressable
                        accessibilityLabel="Add item via scanner"
                        accessibilityRole="button"
                        onPress={openScanner}
                        style={({ pressed }) => [
                          styles.ghostActionButton,
                          pressed && styles.pressed,
                        ]}>
                        <MaterialIcons color="#526049" name="qr-code-scanner" size={18} />
                        <Text style={styles.ghostActionText}>Scan</Text>
                      </Pressable>
                    </View>
                  )}
                </View>

                {activeTab === 'subSpaces' && showCreateSpace && (
                  <AddSubSpaceForm
                    canSubmit={Boolean(accessToken && resolvedHouseholdId)}
                    creating={creating}
                    onSubmit={submitSpace}
                    resetKey={formResetKey}
                  />
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
          loading={loading}
          onAddItem={openAddItem}
          onCreateSubSpace={() => setShowCreateSpace(true)}
          onItemPress={openItem}
          onRefresh={() => loadOverview({ refresh: true })}
          onScan={openScanner}
          onSpacePress={openSpace}
          refreshing={refreshing}
        />
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

function firstParam(value?: string | string[]) {
  return Array.isArray(value) ? value[0] : value;
}

function parseOverviewTab(value?: string): OverviewTab | null {
  return value === 'items' || value === 'subSpaces' ? value : null;
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
      return 'This household or space was not found.';
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
    backgroundColor: '#fff8f3',
  },
  content: {
    gap: 24,
    paddingTop: 20,
  },
  header: {
    gap: 18,
  },
  titleBlock: {
    gap: 8,
  },
  body: {
    color: '#444841',
    fontSize: 16,
    lineHeight: 24,
  },
  pressed: {
    opacity: 0.78,
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
  itemActions: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 8,
  },
  iconActionButton: {
    alignItems: 'center',
    backgroundColor: '#faf2ed',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    height: 42,
    justifyContent: 'center',
    width: 42,
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
});
