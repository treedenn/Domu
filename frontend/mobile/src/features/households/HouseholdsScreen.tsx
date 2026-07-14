import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { router } from 'expo-router';
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
import {
  createHousehold,
  getHouseholds,
  type HouseholdView,
} from '@/features/households/api';
import { AddHouseholdForm } from '@/features/households/components/AddHouseholdForm';
import { HouseholdsList } from '@/features/households/components/HouseholdsList';
import { AppTopBar, type AppTopBarAction } from '@/ui/AppTopBar';

export default function HouseholdsScreen() {
  const { accessToken, clearTokenResponse } = useAuthSession();
  const [households, setHouseholds] = useState<HouseholdView[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [refreshing, setRefreshing] = useState(false);
  const [creating, setCreating] = useState(false);
  const [formResetKey, setFormResetKey] = useState(0);

  const returnToSignIn = useCallback(async () => {
    await clearTokenResponse();
    router.replace('/');
  }, [clearTokenResponse]);

  const householdCountLabel = useMemo(() => {
    if (households.length === 1) {
      return '1 household';
    }

    return `${households.length} households`;
  }, [households.length]);

  const loadHouseholds = useCallback(
    async ({ refresh = false } = {}) => {
      if (!accessToken) {
        setHouseholds([]);
        setError('Sign in to view your households.');
        return;
      }

      if (refresh) {
        setRefreshing(true);
      } else {
        setLoading(true);
      }

      setError(null);

      try {
        const nextHouseholds = await getHouseholds({ accessToken });
        setHouseholds(nextHouseholds);
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
    [accessToken, returnToSignIn],
  );

  useEffect(() => {
    loadHouseholds();
  }, [loadHouseholds]);

  const submitHousehold = useCallback(async (name: string, ownerDisplayName: string) => {
    const trimmedName = name.trim();
    const trimmedOwnerDisplayName = ownerDisplayName.trim();

    if (!accessToken || !trimmedName || !trimmedOwnerDisplayName) {
      return;
    }

    setCreating(true);
    setError(null);

    try {
      const household = await createHousehold(
        { name: trimmedName, ownerDisplayName: trimmedOwnerDisplayName },
        { accessToken },
      );
      setHouseholds((currentHouseholds) => [household, ...currentHouseholds]);
      setFormResetKey((key) => key + 1);
    } catch (exception) {
      if (isExpiredSessionError(exception)) {
        await returnToSignIn();
        return;
      }

      setError(getUserFacingError(exception));
    } finally {
      setCreating(false);
    }
  }, [accessToken, returnToSignIn]);

  const signInAgain = useCallback(() => {
    returnToSignIn();
  }, [returnToSignIn]);

  const openHousehold = useCallback((household: HouseholdView) => {
    router.push({
      pathname: '/households/[householdId]',
      params: { householdId: household.id },
    });
  }, []);

  const topBarActions = useMemo<AppTopBarAction[]>(
    () => [
      {
        disabled: loading || refreshing || !accessToken,
        icon: 'refresh',
        label: 'Refresh',
        onPress: () => loadHouseholds({ refresh: true }),
      },
    ],
    [accessToken, loadHouseholds, loading, refreshing],
  );

  return (
    <SafeAreaView style={styles.safeArea}>
      <KeyboardAvoidingView
        behavior={Platform.select({ ios: 'padding', default: undefined })}
        style={styles.screen}>
        <HouseholdsList
          error={error}
          households={households}
          listHeader={
            <HouseholdsListHeader
              accessToken={accessToken}
              creating={creating}
              error={error}
              householdCountLabel={householdCountLabel}
              loading={loading}
              onSignInAgain={signInAgain}
              onSubmitHousehold={submitHousehold}
              resetKey={formResetKey}
              topBarActions={topBarActions}
            />
          }
          loading={loading}
          onHouseholdPress={openHousehold}
          onRefresh={() => loadHouseholds({ refresh: true })}
          refreshing={refreshing}
        />
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

function HouseholdsListHeader({
  accessToken,
  creating,
  error,
  householdCountLabel,
  loading,
  onSignInAgain,
  onSubmitHousehold,
  resetKey,
  topBarActions,
}: {
  accessToken: string | null;
  creating: boolean;
  error: string | null;
  householdCountLabel: string;
  loading: boolean;
  onSignInAgain: () => void;
  onSubmitHousehold: (name: string, ownerDisplayName: string) => void;
  resetKey: number;
  topBarActions: AppTopBarAction[];
}) {
  return (
    <View style={styles.content}>
      <View style={styles.header}>
        <AppTopBar actions={topBarActions} subtitle="Domu" title="Your Households" />
        <Text style={styles.body}>
          Choose a home to manage its spaces and inventory, or add a new household.
        </Text>
      </View>

      <AddHouseholdForm
        canSubmit={Boolean(accessToken)}
        creating={creating}
        onSubmit={onSubmitHousehold}
        resetKey={resetKey}
      />

      <View style={styles.sectionHeader}>
        <Text style={styles.sectionTitle}>Households</Text>
        <Text style={styles.sectionMeta}>{householdCountLabel}</Text>
      </View>

      {error && (
        <View style={styles.errorPanel}>
          <MaterialIcons color="#944931" name="error-outline" size={22} />
          <View style={styles.errorCopy}>
            <Text style={styles.errorTitle}>Could not load households</Text>
            <Text style={styles.errorText}>{error}</Text>
            {!accessToken && (
              <Pressable
                accessibilityRole="button"
                onPress={onSignInAgain}
                style={({ pressed }) => [styles.secondaryButton, pressed && styles.pressed]}>
                <Text style={styles.secondaryButtonText}>Sign in</Text>
              </Pressable>
            )}
          </View>
        </View>
      )}

      {loading && (
        <View style={styles.loadingPanel}>
          <ActivityIndicator color="#526049" />
          <Text style={styles.loadingText}>Loading households</Text>
        </View>
      )}
    </View>
  );
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
      return 'The requested household resource was not found.';
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
    gap: 12,
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
  secondaryButton: {
    alignItems: 'center',
    alignSelf: 'flex-start',
    backgroundColor: '#ffffff',
    borderColor: '#944931',
    borderRadius: 8,
    borderWidth: 1,
    justifyContent: 'center',
    minHeight: 40,
    paddingHorizontal: 14,
  },
  secondaryButtonText: {
    color: '#944931',
    fontSize: 14,
    fontWeight: '800',
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
