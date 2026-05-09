import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { router, useLocalSearchParams } from 'expo-router';
import { useCallback, useMemo, useState } from 'react';
import {
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { TimeoutError } from '@/core/async/timeout';
import { ApiError } from '@/core/http/apiClient';
import { useAuthSession } from '@/features/auth/authSession';
import {
  createItem,
  ItemContainerType,
  ItemUnit,
  type CreateItemRequest,
} from '@/features/items/api';
import {
  ManualItemForm,
  type ManualItemFormValue,
} from '@/features/items/components/ManualItemForm';
import { AppTopBar, type AppTopBarAction } from '@/ui/AppTopBar';

const scannerRoute = '/households/[householdId]/items/scanner' as never;

export default function AddItemScreen() {
  const { householdId, spaceId, barcode, name } = useLocalSearchParams<{
    householdId?: string | string[];
    spaceId?: string | string[];
    barcode?: string | string[];
    name?: string | string[];
  }>();
  const resolvedHouseholdId = firstParam(householdId);
  const resolvedSpaceId = firstParam(spaceId);
  const { accessToken, clearTokenResponse } = useAuthSession();
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [formResetKey, setFormResetKey] = useState(0);

  const returnToSignIn = useCallback(async () => {
    await clearTokenResponse();
    router.replace('/');
  }, [clearTokenResponse]);

  const submitItem = useCallback(
    async (form: ManualItemFormValue) => {
      if (!accessToken || !resolvedHouseholdId || !resolvedSpaceId) {
        setError('Open a space before adding items.');
        return;
      }

      const trimmedName = form.name.trim();

      if (!trimmedName) {
        setError('Enter an item name.');
        return;
      }

      setSaving(true);
      setError(null);

      const quantity = Math.max(Number(form.quantity) || 1, 1);
      const request: CreateItemRequest = {
        barcode: form.barcode.trim() || null,
        category: form.category.trim() || null,
        entries: [
          {
            containerType: ItemContainerType.Unspecified,
            currentQuantity: quantity,
            initialQuantity: quantity,
            state: form.state,
            unit: ItemUnit.Piece,
          },
        ],
        name: trimmedName,
      };

      try {
        await createItem(resolvedHouseholdId, resolvedSpaceId, request, { accessToken });
        setFormResetKey((key) => key + 1);
        router.back();
      } catch (exception) {
        if (isExpiredSessionError(exception)) {
          await returnToSignIn();
          return;
        }

        setError(getUserFacingError(exception));
      } finally {
        setSaving(false);
      }
    },
    [accessToken, resolvedHouseholdId, resolvedSpaceId, returnToSignIn],
  );

  const openScannerSelection = useCallback(() => {
    if (!resolvedHouseholdId || !resolvedSpaceId) {
      return;
    }

    router.push({
      pathname: scannerRoute,
      params: { householdId: resolvedHouseholdId, spaceId: resolvedSpaceId },
    });
  }, [resolvedHouseholdId, resolvedSpaceId]);

  const topBarActions = useMemo<AppTopBarAction[]>(
    () => [
      {
        disabled: !resolvedHouseholdId || !resolvedSpaceId,
        icon: 'qr-code-scanner',
        label: 'Barcode Scanner',
        onPress: openScannerSelection,
      },
    ],
    [openScannerSelection, resolvedHouseholdId, resolvedSpaceId],
  );

  return (
    <SafeAreaView style={styles.safeArea}>
      <KeyboardAvoidingView
        behavior={Platform.select({ ios: 'padding', default: undefined })}
        style={styles.screen}>
        <View style={styles.contentTopBar}>
          <AppTopBar
            actions={topBarActions}
            onBack={() => router.back()}
            subtitle="Manual entry"
            title="Add New Item"
          />
        </View>

        <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
          <ManualItemForm
            initialBarcode={firstParam(barcode) ?? ''}
            initialName={firstParam(name) ?? ''}
            onSubmit={submitItem}
            resetKey={formResetKey}
            saving={saving}
          />

          {error ? (
            <View style={styles.errorPanel}>
              <MaterialIcons color="#944931" name="error-outline" size={22} />
              <Text style={styles.errorText}>{error}</Text>
            </View>
          ) : null}
        </ScrollView>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

function firstParam(value?: string | string[]) {
  return Array.isArray(value) ? value[0] : value;
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
  screen: {
    flex: 1,
    backgroundColor: '#fff8f3',
  },
  contentTopBar: {
    paddingHorizontal: 20,
    paddingTop: 14,
  },
  content: {
    gap: 24,
    padding: 20,
    paddingBottom: 36,
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
});
