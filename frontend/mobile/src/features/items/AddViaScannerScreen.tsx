import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import {
  CameraView,
  scanFromURLAsync,
  useCameraPermissions,
  type BarcodeScanningResult,
  type BarcodeType,
} from 'expo-camera';
import * as ImagePicker from 'expo-image-picker';
import { router, useLocalSearchParams } from 'expo-router';
import { memo, type ComponentProps, useCallback, useRef, useState } from 'react';
import {
  ActivityIndicator,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { AppTopBar } from '@/ui/AppTopBar';

type ScannerMode = 'camera' | 'photo';

const addItemRoute = '/households/[householdId]/items/add' as never;
const basketRoute = '/households/[householdId]/items/basket' as never;
const supportedBarcodeTypes: BarcodeType[] = [
  'ean13',
  'ean8',
  'upc_a',
  'upc_e',
  'code128',
  'code39',
  'code93',
  'itf14',
  'codabar',
  'qr',
];

export default function AddViaScannerScreen() {
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
        <AppTopBar onBack={() => router.back()} subtitle="Scanner" title="Add via Scanner" />
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

        <View style={styles.tipPanel}>
          <View style={styles.tipIcon}>
            <MaterialIcons color="#77331c" name="lightbulb" size={24} />
          </View>
          <View style={styles.tipCopy}>
            <Text style={styles.tipTitle}>Quick Tip</Text>
            <Text style={styles.tipText}>
              Point the camera clearly at the product barcode. The basket lets you review each
              detected item before saving.
            </Text>
          </View>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

type ScannerDraft = {
  barcode: string;
  imageUri?: string | null;
  name: string;
};

const ScannerDraftPanel = memo(function ScannerDraftPanel({
  onSubmit,
}: {
  onSubmit: (mode: ScannerMode, payload: ScannerDraft) => void;
}) {
  const [permission, requestPermission] = useCameraPermissions();
  const cameraRef = useRef<CameraView>(null);
  const [cameraActive, setCameraActive] = useState(false);
  const [processingPhoto, setProcessingPhoto] = useState(false);
  const [scanLocked, setScanLocked] = useState(false);
  const [status, setStatus] = useState<string | null>(null);

  const openCamera = useCallback(async () => {
    if (!permission?.granted) {
      const nextPermission = await requestPermission();

      if (!nextPermission.granted) {
        setStatus('Camera permission is required to scan barcodes.');
        return;
      }
    }

    setScanLocked(false);
    setStatus('Point the camera at a barcode.');
    setCameraActive(true);
  }, [permission?.granted, requestPermission]);

  const handleBarcodeScanned = useCallback(
    async (result: BarcodeScanningResult) => {
      if (scanLocked) {
        return;
      }

      setScanLocked(true);
      let imageUri: string | null = null;

      try {
        const photo = await cameraRef.current?.takePictureAsync({
          quality: 0.75,
          skipProcessing: true,
        });
        imageUri = photo?.uri ?? null;
      } catch {
        imageUri = null;
      }

      setCameraActive(false);
      onSubmit('camera', { barcode: result.data, imageUri, name: '' });
    },
    [onSubmit, scanLocked],
  );

  const pickPhoto = useCallback(async () => {
    setProcessingPhoto(true);
    setStatus(null);

    try {
      const result = await ImagePicker.launchImageLibraryAsync({
        allowsEditing: false,
        mediaTypes: ['images'],
        quality: 0.85,
      });

      if (result.canceled || !result.assets[0]) {
        return;
      }

      const selectedAsset = result.assets[0];
      const scanResults = await scanFromURLAsync(selectedAsset.uri, supportedBarcodeTypes);
      const [scanResult] = scanResults;

      if (!scanResult?.data) {
        setStatus(
          __DEV__
            ? `No barcode was detected in that photo. Expo read ${scanResults.length} results from ${selectedAsset.uri}.`
            : 'No barcode was detected in that photo. Try a closer, sharper crop or type the barcode and continue.',
        );
        return;
      }

      onSubmit('photo', {
        barcode: scanResult.data,
        imageUri: selectedAsset.uri,
        name: '',
      });
    } catch {
      setStatus('Could not read that photo. You can enter the barcode manually and continue.');
    } finally {
      setProcessingPhoto(false);
    }
  }, [onSubmit]);

  return (
    <View style={styles.panel}>
      <View style={styles.tileGrid}>
        <MethodTile
          icon="photo-camera"
          label="Use Camera"
          meta="Live barcode and item scanning"
          onPress={openCamera}
          tone="primary"
        />
        <MethodTile
          icon="photo-library"
          label="Upload Photo"
          meta="Select from your device gallery"
          loading={processingPhoto}
          onPress={pickPhoto}
          tone="tertiary"
        />
      </View>

      {cameraActive ? (
        <View style={styles.cameraPanel}>
          <CameraView
            barcodeScannerSettings={{
              barcodeTypes: supportedBarcodeTypes,
            }}
            facing="back"
            onBarcodeScanned={handleBarcodeScanned}
            ref={cameraRef}
            style={styles.cameraPreview}
          >
            <View style={styles.cameraOverlay}>
              <View style={styles.scanFrame} />
            </View>
          </CameraView>
          <View style={styles.cameraActions}>
            <Text style={styles.cameraHint}>Align the barcode inside the frame.</Text>
            <Pressable
              accessibilityRole="button"
              onPress={() => setCameraActive(false)}
              style={({ pressed }) => [styles.cancelCameraButton, pressed && styles.pressed]}>
              <Text style={styles.cancelCameraText}>Cancel scan</Text>
            </Pressable>
          </View>
        </View>
      ) : null}

      {status ? <Text style={styles.statusText}>{status}</Text> : null}
    </View>
  );
});

function MethodTile({
  icon,
  label,
  loading,
  meta,
  onPress,
  tone,
}: {
  icon: ComponentProps<typeof MaterialIcons>['name'];
  label: string;
  loading?: boolean;
  meta: string;
  onPress: () => void;
  tone: 'primary' | 'tertiary';
}) {
  return (
    <Pressable
      accessibilityRole="button"
      onPress={onPress}
      style={({ pressed }) => [styles.methodTile, pressed && styles.pressed]}>
      <View style={[styles.methodIcon, tone === 'tertiary' && styles.methodIconTertiary]}>
        {loading ? (
          <ActivityIndicator color={tone === 'tertiary' ? '#494740' : '#f8ffee'} />
        ) : (
          <MaterialIcons
            color={tone === 'tertiary' ? '#494740' : '#f8ffee'}
            name={icon}
            size={34}
          />
        )}
      </View>
      <Text style={styles.methodTitle}>{label}</Text>
      <Text style={styles.methodMeta}>{meta}</Text>
    </Pressable>
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
  topBar: {
    alignItems: 'center',
    backgroundColor: '#fff8f3',
    borderBottomColor: '#e8e1dc',
    borderBottomWidth: 1,
    flexDirection: 'row',
    justifyContent: 'space-between',
    paddingHorizontal: 20,
    paddingVertical: 14,
  },
  iconButton: {
    alignItems: 'center',
    borderRadius: 8,
    height: 44,
    justifyContent: 'center',
    width: 44,
  },
  topTitle: {
    color: '#526049',
    fontSize: 24,
    fontWeight: '700',
    letterSpacing: 0,
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
  panel: {
    gap: 16,
  },
  tileGrid: {
    gap: 16,
  },
  methodTile: {
    alignItems: 'center',
    backgroundColor: '#ffffff',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    gap: 8,
    minHeight: 180,
    justifyContent: 'center',
    padding: 24,
    shadowColor: '#5c5854',
    shadowOffset: { height: 4, width: 0 },
    shadowOpacity: 0.05,
    shadowRadius: 12,
  },
  methodIcon: {
    alignItems: 'center',
    backgroundColor: '#6a7961',
    borderRadius: 999,
    height: 64,
    justifyContent: 'center',
    marginBottom: 8,
    width: 64,
  },
  methodIconTertiary: {
    backgroundColor: '#e7e2d9',
  },
  methodTitle: {
    color: '#1e1b18',
    fontSize: 20,
    fontWeight: '700',
  },
  methodMeta: {
    color: '#444841',
    fontSize: 13,
    fontWeight: '600',
    textAlign: 'center',
  },
  cameraPanel: {
    backgroundColor: '#1e1b18',
    borderRadius: 8,
    overflow: 'hidden',
  },
  cameraPreview: {
    aspectRatio: 3 / 4,
    width: '100%',
  },
  cameraOverlay: {
    alignItems: 'center',
    flex: 1,
    justifyContent: 'center',
  },
  scanFrame: {
    borderColor: '#d8e8cb',
    borderRadius: 8,
    borderWidth: 3,
    height: 130,
    width: '72%',
  },
  cameraActions: {
    backgroundColor: '#33302c',
    gap: 10,
    padding: 14,
  },
  cameraHint: {
    color: '#f7efea',
    fontSize: 13,
    fontWeight: '700',
    textAlign: 'center',
  },
  cancelCameraButton: {
    alignItems: 'center',
    alignSelf: 'center',
    borderColor: '#f7efea',
    borderRadius: 8,
    borderWidth: 1,
    minHeight: 40,
    paddingHorizontal: 16,
    justifyContent: 'center',
  },
  cancelCameraText: {
    color: '#f7efea',
    fontSize: 13,
    fontWeight: '800',
  },
  statusText: {
    color: '#76321c',
    fontSize: 13,
    fontWeight: '700',
    lineHeight: 19,
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
  tipPanel: {
    alignItems: 'flex-start',
    backgroundColor: '#ffdbd0',
    borderColor: '#ffb59e',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    gap: 14,
    padding: 16,
  },
  tipIcon: {
    alignItems: 'center',
    backgroundColor: '#fd9d7f',
    borderRadius: 8,
    height: 48,
    justifyContent: 'center',
    width: 48,
  },
  tipCopy: {
    flex: 1,
    gap: 4,
  },
  tipTitle: {
    color: '#3a0b00',
    fontSize: 15,
    fontWeight: '800',
  },
  tipText: {
    color: '#76321c',
    fontSize: 14,
    lineHeight: 20,
  },
  pressed: {
    opacity: 0.78,
  },
});
