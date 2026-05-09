import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import {
  CameraView,
  useCameraPermissions,
  type BarcodeScanningResult,
} from 'expo-camera';
import { memo, useCallback, useEffect, useState } from 'react';
import {
  ActivityIndicator,
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';

import { ConsumableState } from '@/features/items/api';
import { supportedBarcodeTypes } from '@/features/items/scanner/scannerConstants';

const categorySuggestions = ['Kitchen', 'Bathroom', 'Cleaning', 'Office', 'Other'];

export type ManualItemFormValue = {
  barcode: string;
  category: string;
  name: string;
  quantity: string;
  state: ConsumableState;
};

export const ManualItemForm = memo(function ManualItemForm({
  initialBarcode,
  initialName,
  onSubmit,
  resetKey,
  saving,
}: {
  initialBarcode: string;
  initialName: string;
  onSubmit: (value: ManualItemFormValue) => void;
  resetKey: number;
  saving: boolean;
}) {
  const [permission, requestPermission] = useCameraPermissions();
  const [name, setName] = useState(initialName);
  const [barcode, setBarcode] = useState(initialBarcode);
  const [category, setCategory] = useState('Kitchen');
  const [quantity, setQuantity] = useState('1');
  const [cameraActive, setCameraActive] = useState(false);
  const [scanLocked, setScanLocked] = useState(false);
  const [scanStatus, setScanStatus] = useState<string | null>(null);
  const [state, setState] = useState<ConsumableState>(ConsumableState.Unopened);
  const canSave = Boolean(name.trim()) && !saving;

  useEffect(() => {
    setName(initialName);
    setBarcode(initialBarcode);
    setCategory('Kitchen');
    setQuantity('1');
    setState(ConsumableState.Unopened);
  }, [initialBarcode, initialName, resetKey]);

  const decrementQuantity = useCallback(() => {
    setQuantity((current) => String(Math.max((Number(current) || 1) - 1, 1)));
  }, []);

  const incrementQuantity = useCallback(() => {
    setQuantity((current) => String(Math.max(Number(current) || 0, 0) + 1));
  }, []);

  const submit = useCallback(() => {
    onSubmit({ barcode, category, name, quantity, state });
  }, [barcode, category, name, onSubmit, quantity, state]);

  const clearCategory = useCallback(() => {
    setCategory('');
  }, []);

  const openBarcodeScanner = useCallback(async () => {
    if (!permission?.granted) {
      const nextPermission = await requestPermission();

      if (!nextPermission.granted) {
        setScanStatus('Camera permission is required to scan a barcode.');
        return;
      }
    }

    setScanLocked(false);
    setScanStatus('Point the camera at this item barcode.');
    setCameraActive(true);
  }, [permission?.granted, requestPermission]);

  const handleBarcodeScanned = useCallback(
    (result: BarcodeScanningResult) => {
      if (scanLocked) {
        return;
      }

      setScanLocked(true);
      setBarcode(result.data);
      setCameraActive(false);
      setScanStatus(null);
    },
    [scanLocked],
  );

  return (
    <>
      <View style={styles.sectionHeader}>
        <MaterialIcons color="#526049" name="inventory" size={22} />
        <Text style={styles.sectionTitle}>Item Identification</Text>
      </View>

      <View style={styles.fieldGroup}>
        <View style={styles.field}>
          <Text style={styles.label}>Item Name</Text>
          <TextInput
            autoCapitalize="words"
            onChangeText={setName}
            placeholder="Organic Almond Milk"
            placeholderTextColor="#8c8a81"
            returnKeyType="next"
            style={styles.input}
            value={name}
          />
        </View>

        <View style={styles.field}>
          <Text style={styles.label}>Barcode</Text>
          <View style={styles.inputActionRow}>
            <TextInput
              autoCapitalize="none"
              keyboardType="number-pad"
              onChangeText={setBarcode}
              placeholder="Scan or enter code"
              placeholderTextColor="#8c8a81"
              returnKeyType="next"
              style={[styles.input, styles.inputWithAction]}
              value={barcode}
            />
            <Pressable
              accessibilityLabel="Scan barcode"
              accessibilityRole="button"
              onPress={openBarcodeScanner}
              style={({ pressed }) => [styles.inputIconButton, pressed && styles.pressed]}>
              <MaterialIcons color="#526049" name="qr-code-scanner" size={22} />
            </Pressable>
          </View>
          {scanStatus ? <Text style={styles.scanStatusText}>{scanStatus}</Text> : null}
          {cameraActive ? (
            <View style={styles.cameraPanel}>
              <CameraView
                barcodeScannerSettings={{ barcodeTypes: supportedBarcodeTypes }}
                facing="back"
                onBarcodeScanned={handleBarcodeScanned}
                style={styles.cameraPreview}>
                <View style={styles.cameraOverlay}>
                  <View style={styles.scanFrame} />
                </View>
              </CameraView>
              <Pressable
                accessibilityRole="button"
                onPress={() => setCameraActive(false)}
                style={({ pressed }) => [styles.cancelCameraButton, pressed && styles.pressed]}>
                <Text style={styles.cancelCameraText}>Cancel scan</Text>
              </Pressable>
            </View>
          ) : null}
        </View>

        <View style={styles.field}>
          <Text style={styles.label}>Category</Text>
          <View style={styles.fieldInputShell}>
            <TextInput
              autoCapitalize="words"
              onChangeText={setCategory}
              placeholder="Kitchen"
              placeholderTextColor="#8c8a81"
              returnKeyType="next"
              style={styles.shellInput}
              value={category}
            />
            <Pressable
              accessibilityLabel="Clear category"
              accessibilityRole="button"
              disabled={!category}
              onPress={clearCategory}
              style={({ pressed }) => [
                styles.fieldTrailingButton,
                !category && styles.clearInputButtonDisabled,
                pressed && styles.pressed,
              ]}>
              <MaterialIcons color="#526049" name="close" size={20} />
            </Pressable>
          </View>
          <View style={styles.categorySuggestions}>
            {categorySuggestions.map((suggestion) => (
              <Pressable
                accessibilityRole="button"
                key={suggestion}
                onPress={() => setCategory(suggestion)}
                style={({ pressed }) => [
                  styles.categorySuggestionChip,
                  category === suggestion && styles.categorySuggestionChipActive,
                  pressed && styles.pressed,
                ]}>
                <Text
                  style={[
                    styles.categorySuggestionText,
                    category === suggestion && styles.categorySuggestionTextActive,
                  ]}>
                  {suggestion}
                </Text>
              </Pressable>
            ))}
          </View>
        </View>
      </View>

      <View style={styles.sectionHeader}>
        <MaterialIcons color="#526049" name="inventory-2" size={22} />
        <Text style={styles.sectionTitle}>Item Entry</Text>
      </View>

      <View style={styles.entryPanel}>
        <View style={styles.quantityRow}>
          <View style={styles.fieldCompact}>
            <Text style={styles.label}>Quantity</Text>
            <View style={styles.stepper}>
              <Pressable
                accessibilityLabel="Decrease quantity"
                accessibilityRole="button"
                onPress={decrementQuantity}
                style={({ pressed }) => [styles.stepperButton, pressed && styles.pressed]}>
                <MaterialIcons color="#526049" name="remove" size={20} />
              </Pressable>
              <TextInput
                keyboardType="number-pad"
                onChangeText={setQuantity}
                style={styles.quantityInput}
                value={quantity}
              />
              <Pressable
                accessibilityLabel="Increase quantity"
                accessibilityRole="button"
                onPress={incrementQuantity}
                style={({ pressed }) => [styles.stepperButton, pressed && styles.pressed]}>
                <MaterialIcons color="#526049" name="add" size={20} />
              </Pressable>
            </View>
          </View>
          <View style={styles.unitBlock}>
            <Text style={styles.unitLabel}>Unit</Text>
            <Text style={styles.unitValue}>Pieces</Text>
          </View>
        </View>

        <View style={styles.field}>
          <Text style={styles.label}>Consumable State</Text>
          <View style={styles.segmentedControl}>
            <StateButton
              active={state === ConsumableState.Unspecified}
              label="Unknown"
              onPress={() => setState(ConsumableState.Unspecified)}
            />
            <StateButton
              active={state === ConsumableState.Opened}
              label="Opened"
              onPress={() => setState(ConsumableState.Opened)}
            />
            <StateButton
              active={state === ConsumableState.Unopened}
              label="Unopened"
              onPress={() => setState(ConsumableState.Unopened)}
            />
          </View>
        </View>
      </View>

      <Pressable
        accessibilityRole="button"
        disabled={!canSave}
        onPress={submit}
        style={({ pressed }) => [
          styles.saveButton,
          pressed && styles.primaryButtonPressed,
          !canSave && styles.saveButtonDisabled,
        ]}>
        {saving ? (
          <ActivityIndicator color="#ffffff" />
        ) : (
          <>
            <MaterialIcons color="#ffffff" name="save" size={22} />
            <Text style={styles.saveButtonText}>Save Item</Text>
          </>
        )}
      </Pressable>
    </>
  );
});

function StateButton({
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
      accessibilityRole="button"
      onPress={onPress}
      style={({ pressed }) => [
        styles.segmentButton,
        active && styles.segmentButtonActive,
        pressed && styles.pressed,
      ]}>
      <Text style={[styles.segmentText, active && styles.segmentTextActive]}>{label}</Text>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  sectionHeader: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 8,
  },
  sectionTitle: {
    color: '#1e1b18',
    fontSize: 20,
    fontWeight: '700',
    letterSpacing: 0,
  },
  fieldGroup: {
    gap: 16,
  },
  field: {
    gap: 8,
  },
  fieldCompact: {
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
  inputActionRow: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 8,
  },
  inputWithAction: {
    flex: 1,
  },
  fieldInputShell: {
    alignItems: 'center',
    backgroundColor: '#faf2ed',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    minHeight: 52,
    paddingLeft: 14,
    paddingRight: 6,
  },
  shellInput: {
    color: '#1e1b18',
    flex: 1,
    fontSize: 16,
    minHeight: 50,
    padding: 0,
  },
  inputIconButton: {
    alignItems: 'center',
    backgroundColor: '#ffffff',
    borderColor: '#526049',
    borderRadius: 8,
    borderWidth: 1,
    height: 52,
    justifyContent: 'center',
    width: 52,
  },
  fieldTrailingButton: {
    alignItems: 'center',
    borderRadius: 8,
    height: 40,
    justifyContent: 'center',
    width: 40,
  },
  clearInputButtonDisabled: {
    opacity: 0.45,
  },
  categorySuggestions: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  categorySuggestionChip: {
    alignItems: 'center',
    backgroundColor: '#ffffff',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    justifyContent: 'center',
    minHeight: 34,
    paddingHorizontal: 10,
  },
  categorySuggestionChipActive: {
    backgroundColor: '#d8e8cb',
    borderColor: '#526049',
  },
  categorySuggestionText: {
    color: '#444841',
    fontSize: 12,
    fontWeight: '800',
  },
  categorySuggestionTextActive: {
    color: '#121f0d',
  },
  scanStatusText: {
    color: '#76321c',
    fontSize: 13,
    fontWeight: '700',
    lineHeight: 19,
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
  cancelCameraButton: {
    alignItems: 'center',
    backgroundColor: '#33302c',
    justifyContent: 'center',
    minHeight: 46,
  },
  cancelCameraText: {
    color: '#f7efea',
    fontSize: 13,
    fontWeight: '800',
  },
  entryPanel: {
    backgroundColor: '#ffffff',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    gap: 18,
    padding: 16,
    shadowColor: '#5c5854',
    shadowOffset: { height: 4, width: 0 },
    shadowOpacity: 0.05,
    shadowRadius: 12,
  },
  quantityRow: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 16,
    justifyContent: 'space-between',
  },
  stepper: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 12,
  },
  stepperButton: {
    alignItems: 'center',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    height: 40,
    justifyContent: 'center',
    width: 40,
  },
  quantityInput: {
    color: '#1e1b18',
    fontSize: 20,
    fontWeight: '700',
    minWidth: 48,
    padding: 0,
    textAlign: 'center',
  },
  unitBlock: {
    borderLeftColor: '#e8e1dc',
    borderLeftWidth: 1,
    minWidth: 84,
    paddingLeft: 16,
  },
  unitLabel: {
    color: '#757870',
    fontSize: 12,
    fontWeight: '700',
  },
  unitValue: {
    color: '#1e1b18',
    fontSize: 16,
    fontWeight: '600',
    marginTop: 4,
  },
  segmentedControl: {
    backgroundColor: '#f4ede7',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    gap: 4,
    padding: 4,
  },
  segmentButton: {
    alignItems: 'center',
    borderRadius: 8,
    flex: 1,
    justifyContent: 'center',
    minHeight: 38,
  },
  segmentButtonActive: {
    backgroundColor: '#6a7961',
  },
  segmentText: {
    color: '#444841',
    fontSize: 13,
    fontWeight: '700',
  },
  segmentTextActive: {
    color: '#f8ffee',
  },
  saveButton: {
    alignItems: 'center',
    backgroundColor: '#526049',
    borderRadius: 999,
    flexDirection: 'row',
    gap: 8,
    justifyContent: 'center',
    minHeight: 58,
    paddingHorizontal: 18,
  },
  saveButtonDisabled: {
    backgroundColor: '#9ca58f',
  },
  saveButtonText: {
    color: '#ffffff',
    fontSize: 18,
    fontWeight: '700',
  },
  primaryButtonPressed: {
    opacity: 0.86,
  },
  pressed: {
    opacity: 0.78,
  },
});
