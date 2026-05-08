import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { router } from 'expo-router';
import { useCallback, useEffect, useMemo, useState } from 'react';
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
import {
  createHousehold,
  getHouseholds,
  type HouseholdView,
} from '@/features/households/api';

export default function HouseholdsScreen() {
  const { accessToken, clearTokenResponse } = useAuthSession();
  const [households, setHouseholds] = useState<HouseholdView[]>([]);
  const [name, setName] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [refreshing, setRefreshing] = useState(false);
  const [creating, setCreating] = useState(false);
  const trimmedName = name.trim();
  const canCreate = Boolean(accessToken && trimmedName && !creating);

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
        setError(getUserFacingError(exception));
      } finally {
        setLoading(false);
        setRefreshing(false);
      }
    },
    [accessToken],
  );

  useEffect(() => {
    loadHouseholds();
  }, [loadHouseholds]);

  const submitHousehold = useCallback(async () => {
    if (!accessToken || !trimmedName) {
      return;
    }

    setCreating(true);
    setError(null);

    try {
      const household = await createHousehold({ name: trimmedName }, { accessToken });
      setHouseholds((currentHouseholds) => [household, ...currentHouseholds]);
      setName('');
    } catch (exception) {
      setError(getUserFacingError(exception));
    } finally {
      setCreating(false);
    }
  }, [accessToken, trimmedName]);

  const signInAgain = useCallback(() => {
    clearTokenResponse();
    router.replace('/');
  }, [clearTokenResponse]);

  return (
    <SafeAreaView style={styles.safeArea}>
      <KeyboardAvoidingView
        behavior={Platform.select({ ios: 'padding', default: undefined })}
        style={styles.screen}>
        <FlatList
          ListHeaderComponent={
            <View style={styles.content}>
              <View style={styles.header}>
                <View style={styles.headerTop}>
                  <View>
                    <Text style={styles.eyebrow}>Domu</Text>
                    <Text style={styles.title}>Your Households</Text>
                  </View>
                  <Pressable
                    accessibilityLabel="Refresh households"
                    accessibilityRole="button"
                    disabled={loading || refreshing || !accessToken}
                    onPress={() => loadHouseholds({ refresh: true })}
                    style={({ pressed }) => [
                      styles.iconButton,
                      pressed && styles.pressed,
                      (!accessToken || loading || refreshing) && styles.disabledButton,
                    ]}>
                    <MaterialIcons color="#526049" name="refresh" size={22} />
                  </Pressable>
                </View>
                <Text style={styles.body}>
                  Choose a home to manage its spaces and inventory, or add a new household.
                </Text>
              </View>

              <View style={styles.formPanel}>
                <View style={styles.formHeader}>
                  <MaterialIcons color="#526049" name="add-home" size={22} />
                  <Text style={styles.formTitle}>Add Household</Text>
                </View>

                <View style={styles.field}>
                  <Text style={styles.label}>Household name</Text>
                  <TextInput
                    autoCapitalize="words"
                    onChangeText={setName}
                    onSubmitEditing={submitHousehold}
                    placeholder="Oak Street Home"
                    placeholderTextColor="#8c8a81"
                    returnKeyType="done"
                    style={styles.input}
                    value={name}
                  />
                </View>

                <Pressable
                  accessibilityRole="button"
                  disabled={!canCreate}
                  onPress={submitHousehold}
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
                      <Text style={styles.primaryButtonText}>Create household</Text>
                    </>
                  )}
                </Pressable>
              </View>

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
                        onPress={signInAgain}
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
          }
          contentContainerStyle={styles.listContent}
          data={households}
          keyExtractor={(item) => item.id}
          keyboardShouldPersistTaps="handled"
          ListEmptyComponent={!loading && !error ? <EmptyHouseholds /> : null}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={() => loadHouseholds({ refresh: true })}
              tintColor="#526049"
            />
          }
          renderItem={({ item }) => <HouseholdCard household={item} />}
          ItemSeparatorComponent={() => <View style={styles.separator} />}
        />
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

function HouseholdCard({ household }: { household: HouseholdView }) {
  const openHousehold = useCallback(() => {
    router.push({
      pathname: '/households/[householdId]',
      params: { householdId: household.id },
    });
  }, [household.id]);

  return (
    <Pressable
      accessibilityRole="button"
      onPress={openHousehold}
      style={({ pressed }) => [styles.householdCard, pressed && styles.cardPressed]}>
      <View style={styles.householdIcon}>
        <MaterialIcons color="#526049" name="home" size={24} />
      </View>
      <View style={styles.householdContent}>
        <Text numberOfLines={1} style={styles.householdName}>
          {household.name}
        </Text>
        <Text style={styles.householdDetails}>{formatSubscription(household)}</Text>
      </View>
      <MaterialIcons color="#757870" name="chevron-right" size={24} />
    </Pressable>
  );
}

function EmptyHouseholds() {
  return (
    <View style={styles.emptyPanel}>
      <View style={styles.emptyIcon}>
        <MaterialIcons color="#526049" name="home-work" size={28} />
      </View>
      <Text style={styles.emptyTitle}>No households yet</Text>
      <Text style={styles.emptyText}>Create your first household to start organizing spaces.</Text>
    </View>
  );
}

function formatSubscription(household: HouseholdView) {
  const plan = household.subscriptionPlan === 2 ? 'Premium' : 'Free';
  const status = household.subscriptionStatus === 2 ? 'Cancellation scheduled' : 'Active';

  return `${plan} plan - ${status}`;
}

function getUserFacingError(exception: unknown) {
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
  content: {
    gap: 24,
    paddingTop: 20,
  },
  header: {
    gap: 12,
  },
  headerTop: {
    alignItems: 'flex-start',
    flexDirection: 'row',
    justifyContent: 'space-between',
    gap: 16,
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
    marginTop: 4,
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
  separator: {
    height: 12,
  },
  householdCard: {
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
  householdIcon: {
    alignItems: 'center',
    backgroundColor: '#d8e8cb',
    borderRadius: 8,
    height: 46,
    justifyContent: 'center',
    width: 46,
  },
  householdContent: {
    flex: 1,
    gap: 4,
    minWidth: 0,
  },
  householdName: {
    color: '#1e1b18',
    fontSize: 17,
    fontWeight: '800',
    letterSpacing: 0,
  },
  householdDetails: {
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
    marginTop: 12,
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
});
