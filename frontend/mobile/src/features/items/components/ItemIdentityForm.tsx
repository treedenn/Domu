import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { useCallback, useEffect, useState, type ReactNode } from 'react';
import {
  ActivityIndicator,
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';

import type { ItemView } from '@/features/items/api';

export type ItemIdentityFormValue = {
  barcode: string;
  category: string;
  name: string;
};

type ItemIdentityFormProps = {
  item: ItemView;
  onCancel: () => void;
  onSubmit: (value: ItemIdentityFormValue) => void;
  saving: boolean;
};

export function ItemIdentityForm({
  item,
  onCancel,
  onSubmit,
  saving,
}: ItemIdentityFormProps) {
  const [name, setName] = useState(item.name);
  const [category, setCategory] = useState(item.category ?? '');
  const [barcode, setBarcode] = useState(item.barcode ?? '');
  const canSave = Boolean(name.trim()) && !saving;

  useEffect(() => {
    setName(item.name);
    setCategory(item.category ?? '');
    setBarcode(item.barcode ?? '');
  }, [item]);

  const submit = useCallback(() => {
    onSubmit({ barcode, category, name });
  }, [barcode, category, name, onSubmit]);

  return (
    <View style={styles.formStack}>
      <FormField label="Name">
        <TextInput
          autoCapitalize="words"
          onChangeText={setName}
          placeholder="Item name"
          placeholderTextColor="#8c8a81"
          style={styles.input}
          value={name}
        />
      </FormField>
      <FormField label="Category">
        <TextInput
          autoCapitalize="words"
          onChangeText={setCategory}
          placeholder="Kitchen"
          placeholderTextColor="#8c8a81"
          style={styles.input}
          value={category}
        />
      </FormField>
      <FormField label="Barcode">
        <TextInput
          autoCapitalize="none"
          keyboardType="number-pad"
          onChangeText={setBarcode}
          placeholder="Barcode"
          placeholderTextColor="#8c8a81"
          style={styles.input}
          value={barcode}
        />
      </FormField>
      <FormActions canSave={canSave} onCancel={onCancel} onSave={submit} saving={saving} />
    </View>
  );
}

function FormField({ children, label }: { children: ReactNode; label: string }) {
  return (
    <View style={styles.field}>
      <Text style={styles.label}>{label}</Text>
      {children}
    </View>
  );
}

function FormActions({
  canSave,
  onCancel,
  onSave,
  saving,
}: {
  canSave: boolean;
  onCancel: () => void;
  onSave: () => void;
  saving: boolean;
}) {
  return (
    <View style={styles.formActions}>
      <Pressable
        accessibilityRole="button"
        onPress={onCancel}
        style={({ pressed }) => [styles.cancelButton, pressed && styles.pressed]}>
        <Text style={styles.cancelButtonText}>Cancel</Text>
      </Pressable>
      <Pressable
        accessibilityRole="button"
        disabled={!canSave}
        onPress={onSave}
        style={({ pressed }) => [
          styles.saveButton,
          pressed && styles.pressed,
          !canSave && styles.disabledButton,
        ]}>
        {saving ? (
          <ActivityIndicator color="#ffffff" />
        ) : (
          <>
            <MaterialIcons color="#ffffff" name="save" size={18} />
            <Text style={styles.saveButtonText}>Save</Text>
          </>
        )}
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  formStack: {
    gap: 14,
  },
  field: {
    flex: 1,
    gap: 8,
  },
  label: {
    color: '#444841',
    fontSize: 13,
    fontWeight: '800',
  },
  input: {
    backgroundColor: '#faf2ed',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    color: '#1e1b18',
    fontSize: 15,
    minHeight: 46,
    paddingHorizontal: 12,
  },
  formActions: {
    flexDirection: 'row',
    gap: 10,
  },
  cancelButton: {
    alignItems: 'center',
    backgroundColor: '#ffffff',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    flex: 1,
    justifyContent: 'center',
    minHeight: 46,
  },
  cancelButtonText: {
    color: '#444841',
    fontSize: 14,
    fontWeight: '800',
  },
  saveButton: {
    alignItems: 'center',
    backgroundColor: '#526049',
    borderRadius: 8,
    flex: 1,
    flexDirection: 'row',
    gap: 6,
    justifyContent: 'center',
    minHeight: 46,
  },
  saveButtonText: {
    color: '#ffffff',
    fontSize: 14,
    fontWeight: '800',
  },
  disabledButton: {
    opacity: 0.5,
  },
  pressed: {
    opacity: 0.78,
  },
});
