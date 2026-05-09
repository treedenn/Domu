import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { router, useLocalSearchParams } from 'expo-router';
import { useCallback } from 'react';
import { Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { ScannerDraftPanel } from '@/features/items/scanner/ScannerDraftPanel';
import { ScannerTipPanel } from '@/features/items/scanner/ScannerTipPanel';
import type { ScannerDraft, ScannerMode } from '@/features/items/scanner/types';
import { AppTopBar } from '@/ui/AppTopBar';

const addItemRoute = '/households/[householdId]/items/add' as never;
const basketRoute = '/households/[householdId]/items/basket' as never;

export default function ScannerScreen() {
  const { householdId, spaceId } = useLocalSearchParams<{
    householdId?: string | string[];
    spaceId?: string | string[];
  }>();
  const resolvedHouseholdId = firstParam(householdId);
  const resolvedSpaceId = firstParam(spaceId);

  const openManualEntry = useCallback(() => {
    if (!resolvedHouseholdId || !resolvedSpaceId) {
      return;
    }

    router.replace({
      pathname: addItemRoute,
      params: { householdId: resolvedHouseholdId, spaceId: resolvedSpaceId },
    });
  }, [resolvedHouseholdId, resolvedSpaceId]);

  const openBasket = useCallback(
    (mode: ScannerMode, payload: ScannerDraft) => {
      if (!resolvedHouseholdId || !resolvedSpaceId) {
        return;
      }

      const pendingItems = [
        {
          barcode: payload.barcode.trim() || null,
          category: 'Kitchen',
          imageUri: payload.imageUri ?? null,
          name: payload.name.trim(),
          quantity: 1,
          source: mode,
        },
      ];

      router.push({
        pathname: basketRoute,
        params: {
          householdId: resolvedHouseholdId,
          items: JSON.stringify(pendingItems),
          spaceId: resolvedSpaceId,
        },
      });
    },
    [resolvedHouseholdId, resolvedSpaceId],
  );

  return (
    <SafeAreaView style={styles.safeArea}>
      <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
        <AppTopBar onBack={() => router.back()} subtitle="Scan to basket" title="Barcode Scanner" />
        <View style={styles.hero}>
          <Text style={styles.heroTitle}>Choose Input Method</Text>
          <Text style={styles.heroText}>Select how you would like to document your new items.</Text>
        </View>

        <ScannerDraftPanel onSubmit={openBasket} />

        <View style={styles.dividerRow}>
          <View style={styles.divider} />
          <Text style={styles.dividerText}>or</Text>
          <View style={styles.divider} />
        </View>

        <Pressable
          accessibilityRole="button"
          onPress={openManualEntry}
          style={({ pressed }) => [styles.manualButton, pressed && styles.pressed]}>
          <MaterialIcons color="#1e1b18" name="edit-note" size={22} />
          <Text style={styles.manualButtonText}>Enter Manually</Text>
        </Pressable>

        <ScannerTipPanel />
      </ScrollView>
    </SafeAreaView>
  );
}

function firstParam(value?: string | string[]) {
  return Array.isArray(value) ? value[0] : value;
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: '#fff8f3',
  },
  content: {
    gap: 24,
    padding: 20,
    paddingBottom: 36,
  },
  hero: {
    alignItems: 'center',
    gap: 8,
  },
  heroTitle: {
    color: '#1e1b18',
    fontSize: 32,
    fontWeight: '700',
    letterSpacing: 0,
    textAlign: 'center',
  },
  heroText: {
    color: '#444841',
    fontSize: 16,
    lineHeight: 24,
    textAlign: 'center',
  },
  dividerRow: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 16,
    opacity: 0.7,
  },
  divider: {
    backgroundColor: '#757870',
    flex: 1,
    height: 1,
  },
  dividerText: {
    color: '#444841',
    fontSize: 12,
    fontWeight: '800',
    textTransform: 'uppercase',
  },
  manualButton: {
    alignItems: 'center',
    backgroundColor: '#faf2ed',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    gap: 8,
    justifyContent: 'center',
    minHeight: 52,
  },
  manualButtonText: {
    color: '#1e1b18',
    fontSize: 16,
    fontWeight: '700',
  },
  pressed: {
    opacity: 0.78,
  },
});
