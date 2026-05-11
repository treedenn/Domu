import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { router, useLocalSearchParams } from 'expo-router';
import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  ActivityIndicator,
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
import { AddSpaceForm } from '@/features/households/components/AddSpaceForm';
import { DashboardSummaryMetrics } from '@/features/households/components/DashboardSummaryMetrics';
import { SpacesList } from '@/features/households/components/SpacesList';
import { createSpace, getSpaces, type SpaceView } from '@/features/spaces/api';
import { AppTopBar, type AppTopBarAction } from '@/ui/AppTopBar';

export default function HouseholdDashboardScreen() {
  const { householdId } = useLocalSearchParams<{ householdId?: string | string[] }>();
  const resolvedHouseholdId = firstParam(householdId);
  const { accessToken, clearTokenResponse } = useAuthSession();
  const [household, setHousehold] = useState<HouseholdView | null>(null);
  const [spaces, setSpaces] = useState<SpaceView[]>([]);
  const [totalSpaces, setTotalSpaces] = useState(0);
  const [loading, setLoading] = useState(false);
  const [refreshing, setRefreshing] = useState(false);
  const [creating, setCreating] = useState(false);
  const [showCreateSpace, setShowCreateSpace] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [formResetKey, setFormResetKey] = useState(0);

  const returnToSignIn = useCallback(async () => {
    await clearTokenResponse();
    router.replace('/');
  }, [clearTokenResponse]);

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
    loadDashboard();
  }, [loadDashboard]);

  const submitSpace = useCallback(async (name: string, description: string) => {
    const trimmedSpaceName = name.trim();
    const trimmedSpaceDescription = description.trim();

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
  }, [accessToken, resolvedHouseholdId, returnToSignIn]);

  const topBarActions = useMemo<AppTopBarAction[]>(
    () => [
      {
        disabled: loading || refreshing || !accessToken,
        icon: 'refresh',
        label: 'Refresh',
        onPress: () => loadDashboard({ refresh: true }),
      },
    ],
    [accessToken, loadDashboard, loading, refreshing],
  );

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

  return (
    <SafeAreaView style={styles.safeArea}>
      <SpacesList
        error={error}
        listHeader={
          <View style={styles.content}>
            <View style={styles.header}>
              <AppTopBar
                actions={topBarActions}
                backAccessibilityLabel="Back to households"
                onBack={() => router.back()}
                subtitle="Household"
                title={household?.name ?? 'Household Dashboard'}
              />

              <View style={styles.titleBlock}>
                <Text style={styles.body}>Overview of spaces and inventory in this home.</Text>
              </View>
            </View>

            <DashboardSummaryMetrics itemCount={itemCount} totalSpaces={totalSpaces} />

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
                <AddSpaceForm
                  canSubmit={Boolean(accessToken && resolvedHouseholdId)}
                  creating={creating}
                  onSubmit={submitSpace}
                  resetKey={formResetKey}
                />
              )}
            </View>
          </View>
        }
        loading={loading}
        onCreate={() => setShowCreateSpace(true)}
        onRefresh={() => loadDashboard({ refresh: true })}
        onSpacePress={openSpace}
        refreshing={refreshing}
        spaces={spaces}
      />
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

function isExpiredSessionError(exception: unknown) {
  return exception instanceof ApiError && exception.status === 401;
}

const styles = StyleSheet.create({
  safeArea: {
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
});
