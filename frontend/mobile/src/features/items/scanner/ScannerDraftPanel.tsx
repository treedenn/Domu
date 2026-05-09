import {
  CameraView,
  scanFromURLAsync,
  useCameraPermissions,
  type BarcodeScanningResult,
} from 'expo-camera';
import * as ImagePicker from 'expo-image-picker';
import { memo, useCallback, useRef, useState } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';

import { MethodTile } from '@/features/items/scanner/MethodTile';
import { supportedBarcodeTypes } from '@/features/items/scanner/scannerConstants';
import type { ScannerDraft, ScannerMode } from '@/features/items/scanner/types';

export const ScannerDraftPanel = memo(function ScannerDraftPanel({
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
          loading={processingPhoto}
          meta="Select from your device gallery"
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
            style={styles.cameraPreview}>
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

const styles = StyleSheet.create({
  panel: {
    gap: 16,
  },
  tileGrid: {
    gap: 16,
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
    justifyContent: 'center',
    minHeight: 40,
    paddingHorizontal: 16,
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
  pressed: {
    opacity: 0.78,
  },
});
