import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { router, useLocalSearchParams } from 'expo-router';
import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  FlatList,
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
import { createSpace, getSpaces, type SpaceView } from '@/features/spaces/api';

export default function HouseholdDashboardScreen() {
  const { householdId } = useLocalSearchParams<{ householdId?: string | string[] }>();
  const resolvedHouseholdId = firstParam(householdId);
  const { accessToken } = useAuthSession();
  const [household, setHousehold] = useState<HouseholdView | null>(null);
  const [spaces, setSpaces] = useState<SpaceView[]>([]);
  const [totalSpaces, setTotalSpaces] = useState(0);
  const [loading, setLoading] = useState(false);
  const [refreshing, setRefreshing] = useState(false);
  const [creating, setCreating] = useState(false);
  const [showCreateSpace, setShowCreateSpace] = useState(false);
  const [spaceName, setSpaceName] = useState('');
  const [spaceDescription, setSpaceDescription] = useState('');
  const [error, setError] = useState<string | null>(null);
  const trimmedSpaceName = spaceName.trim();
  const trimmedSpaceDescription = spaceDescription.trim();
  const canCreateSpace = Boolean(accessToken && resolvedHouseholdId && trimmedSpaceName && !creating);

  const itemCount = useMemo(
    () => spaces.reduce((total, space) => total + (space.items?.count ?? 0), 0),
    [spaces],
  );

  const loadDashboard = useCallback(
    async ({ refresh = false } = {}) => {
      if (!accessToken || !resolvedHouseholdId) {
        setError('Sign in and choose a household to view the dashboard.');
        setHousehold(null);
        setSpaces([]);
        setTotalSpaces(0);
        return;
      }

      if (refresh) {
        setRefreshing(true);
      } else {
        setLoading(true);
      }

      setError(null);

      try {
        const [nextHousehold, spacesPage] = await Promise.all([
          getHousehold(resolvedHouseholdId, { accessToken }),
          getSpaces(
            resolvedHouseholdId,
            {
              includeChildSpaceCount: true,
              includeItemCount: true,
              pageSize: 6,
              parentId: null,
            },
            { accessToken },
          ),
        ]);

        setHousehold(nextHousehold);
        setSpaces(spacesPage.spaces);
        setTotalSpaces(spacesPage.totalCount);
      } catch (exception) {
        setError(getUserFacingError(exception));
      } finally {
        setLoading(false);
        setRefreshing(false);
      }
    },
    [accessToken, resolvedHouseholdId],
  );

  useEffect(() => {
    loadDashboard();
  }, [loadDashboard]);

  const submitSpace = useCallback(async () => {
    if (!accessToken || !resolvedHouseholdId || !trimmedSpaceName) {
      return;
    }

    setCreating(true);
    setError(null);

    try {
      const space = await createSpace(
        resolvedHouseholdId,
        {
          description: trimmedSpaceDescription || null,
          name: trimmedSpaceName,
          parentId: null,
        },
        { accessToken },
      );

      setSpaces((currentSpaces) => [space, ...currentSpaces]);
      setTotalSpaces((currentTotal) => currentTotal + 1);
      setSpaceName('');
      setSpaceDescription('');
      setShowCreateSpace(false);
    } catch (exception) {
      setError(getUserFacingError(exception));
    } finally {
      setCreating(false);
    }
  }, [accessToken, resolvedHouseholdId, trimmedSpaceDescription, trimmedSpaceName]);

  return (
    <SafeAreaView style={styles.safeArea}>
      <FlatList
        ListHeaderComponent={
          <View style={styles.content}>
            <View style={styles.header}>
              <View style={styles.headerActions}>
                <Pressable
                  accessibilityLabel="Back to households"
                  accessibilityRole="button"
                  onPress={() => router.back()}
                  style={({ pressed }) => [styles.iconButton, pressed && styles.pressed]}>
                  <MaterialIcons color="#526049" name="arrow-back" size={22} />
                </Pressable>
                <Pressable
                  accessibilityLabel="Refresh household dashboard"
                  accessibilityRole="button"
                  disabled={loading || refreshing || !accessToken}
                  onPress={() => loadDashboard({ refresh: true })}
                  style={({ pressed }) => [
                    styles.iconButton,
                    pressed && styles.pressed,
                    (!accessToken || loading || refreshing) && styles.disabledButton,
                  ]}>
                  <MaterialIcons color="#526049" name="refresh" size={22} />
                </Pressable>
              </View>

              <View style={styles.titleBlock}>
                <Text style={styles.eyebrow}>Household</Text>
                <Text style={styles.title}>{household?.name ?? 'Household Dashboard'}</Text>
                <Text style={styles.body}>Overview of spaces and inventory in this home.</Text>
              </View>
            </View>

            <View style={styles.summaryGrid}>
              <SummaryMetric icon="inventory-2" label="Spaces" value={totalSpaces} />
              <SummaryMetric icon="kitchen" label="Items" value={itemCount} />
            </View>

            {error && (
              <View style={styles.errorPanel}>
                <MaterialIcons color="#944931" name="error-outline" size={22} />
                <View style={styles.errorCopy}>
                  <Text style={styles.errorTitle}>Could not load dashboard</Text>
                  <Text style={styles.errorText}>{error}</Text>
                </View>
              </View>
            )}

            {loading && (
              <View style={styles.loadingPanel}>
                <ActivityIndicator color="#526049" />
                <Text style={styles.loadingText}>Loading dashboard</Text>
              </View>
            )}

            <View style={styles.spacesSection}>
              <View style={styles.sectionHeader}>
                <View>
                  <Text style={styles.sectionTitle}>Spaces</Text>
                  <Text style={styles.sectionMeta}>{formatCount(totalSpaces, 'space')}</Text>
                </View>
                <Pressable
                  accessibilityLabel={showCreateSpace ? 'Hide add space form' : 'Show add space form'}
                  accessibilityRole="button"
                  onPress={() => setShowCreateSpace((visible) => !visible)}
                  style={({ pressed }) => [styles.ghostActionButton, pressed && styles.pressed]}>
                  <MaterialIcons
                    color="#526049"
                    name={showCreateSpace ? 'close' : 'add'}
                    size={18}
                  />
                  <Text style={styles.ghostActionText}>
                    {showCreateSpace ? 'Close' : 'Add Space'}
                  </Text>
                </Pressable>
              </View>

              {showCreateSpace && (
                <View style={styles.formPanel}>
                  <View style={styles.formHeader}>
                    <MaterialIcons color="#526049" name="create-new-folder" size={22} />
                    <Text style={styles.formTitle}>Add Space</Text>
                  </View>

                  <View style={styles.field}>
                    <Text style={styles.label}>Name</Text>
                    <TextInput
                      autoCapitalize="words"
                      onChangeText={setSpaceName}
                      onSubmitEditing={submitSpace}
                      placeholder="Kitchen"
                      placeholderTextColor="#8c8a81"
                      returnKeyType="next"
                      style={styles.input}
                      value={spaceName}
                    />
                  </View>

                  <View style={styles.field}>
                    <Text style={styles.label}>Description</Text>
                    <TextInput
                      multiline
                      onChangeText={setSpaceDescription}
                      placeholder="Cooking tools and everyday food storage"
                      placeholderTextColor="#8c8a81"
                      style={[styles.input, styles.textArea]}
                      value={spaceDescription}
                    />
                  </View>

                  <Pressable
                    accessibilityRole="button"
                    disabled={!canCreateSpace}
                    onPress={submitSpace}
                    style={({ pressed }) => [
                      styles.primaryButton,
                      pressed && styles.primaryButtonPressed,
                      !canCreateSpace && styles.primaryButtonDisabled,
                    ]}>
                    {creating ? (
                      <ActivityIndicator color="#ffffff" />
                    ) : (
                      <>
                        <MaterialIcons color="#ffffff" name="add" size={20} />
                        <Text style={styles.primaryButtonText}>Create space</Text>
                      </>
                    )}
                  </Pressable>
                </View>
              )}
            </View>
          </View>
        }
        ListHeaderComponentStyle={styles.listHeader}
        contentContainerStyle={styles.listContent}
        data={spaces}
        ItemSeparatorComponent={() => <View style={styles.separator} />}
        keyExtractor={(item) => item.id}
        ListEmptyComponent={
          !loading && !error ? <EmptySpaces onCreate={() => setShowCreateSpace(true)} /> : null
        }
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={() => loadDashboard({ refresh: true })}
            tintColor="#526049"
          />
        }
        renderItem={({ item }) =>
          resolvedHouseholdId ? (
            <SpacePreviewCard householdId={resolvedHouseholdId} space={item} />
          ) : null
        }
      />
    </SafeAreaView>
  );
}

function SummaryMetric({
  icon,
  label,
  value,
}: {
  icon: React.ComponentProps<typeof MaterialIcons>['name'];
  label: string;
  value: number;
}) {
  return (
    <View style={styles.summaryMetric}>
      <MaterialIcons color="#526049" name={icon} size={22} />
      <Text style={styles.summaryValue}>{value}</Text>
      <Text style={styles.summaryLabel}>{label}</Text>
    </View>
  );
}

function SpacePreviewCard({ householdId, space }: { householdId: string; space: SpaceView }) {
  const openSpace = useCallback(() => {
    router.push({
      pathname: '/households/[householdId]/spaces',
      params: { householdId, parentId: space.id },
    });
  }, [householdId, space.id]);

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
        <Text style={styles.spaceDetails}>
          {formatCount(space.childSpaces?.count ?? 0, 'space')} - {formatCount(space.items?.count ?? 0, 'item')}
        </Text>
      </View>
      <MaterialIcons color="#757870" name="chevron-right" size={24} />
    </Pressable>
  );
}

function EmptySpaces({ onCreate }: { onCreate: () => void }) {
  return (
    <View style={styles.emptyPanel}>
      <View style={styles.emptyIcon}>
        <MaterialIcons color="#526049" name="inventory-2" size={28} />
      </View>
      <Text style={styles.emptyTitle}>No spaces yet</Text>
      <Text style={styles.emptyText}>Create the first storage area for this household.</Text>
      <Pressable
        accessibilityRole="button"
        onPress={onCreate}
        style={({ pressed }) => [styles.emptyButton, pressed && styles.pressed]}>
        <Text style={styles.emptyButtonText}>Create space</Text>
      </Pressable>
    </View>
  );
}

function firstParam(value?: string | string[]) {
  return Array.isArray(value) ? value[0] : value;
}

function formatCount(value: number, noun: string) {
  return `${value} ${noun}${value === 1 ? '' : 's'}`;
}

function getUserFacingError(exception: unknown) {
  if (exception instanceof ApiError) {
    if (exception.status === 401) {
      return 'Your session is missing or expired. Sign in again.';
    }

    if (exception.status === 404) {
      return 'This household was not found.';
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
  sectionHeader: {
    alignItems: 'center',
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  spacesSection: {
    gap: 12,
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
    minHeight: 78,
    padding: 14,
  },
  cardPressed: {
    backgroundColor: '#faf2ed',
  },
  spaceIcon: {
    alignItems: 'center',
    backgroundColor: '#d8e8cb',
    borderRadius: 8,
    height: 46,
    justifyContent: 'center',
    width: 46,
  },
  spaceContent: {
    flex: 1,
    gap: 4,
    minWidth: 0,
  },
  spaceName: {
    color: '#1e1b18',
    fontSize: 17,
    fontWeight: '800',
    letterSpacing: 0,
  },
  spaceDetails: {
    color: '#444841',
    fontSize: 13,
    lineHeight: 18,
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
