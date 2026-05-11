import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { memo, useCallback, useEffect, useState } from 'react';
import {
  ActivityIndicator,
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';

type AddHouseholdFormProps = {
  canSubmit: boolean;
  creating: boolean;
  onSubmit: (name: string) => void;
  resetKey: number;
};

export const AddHouseholdForm = memo(function AddHouseholdForm({
  canSubmit,
  creating,
  onSubmit,
  resetKey,
}: AddHouseholdFormProps) {
  const [name, setName] = useState('');
  const canCreate = canSubmit && Boolean(name.trim()) && !creating;

  useEffect(() => {
    setName('');
  }, [resetKey]);

  const submit = useCallback(() => {
    onSubmit(name);
  }, [name, onSubmit]);

  return (
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
          onSubmitEditing={submit}
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
        onPress={submit}
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
  );
});

const styles = StyleSheet.create({
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
});
