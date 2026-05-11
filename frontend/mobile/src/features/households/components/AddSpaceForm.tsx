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

type AddSpaceFormProps = {
  canSubmit: boolean;
  creating: boolean;
  onSubmit: (name: string, description: string) => void;
  resetKey: number;
};

export const AddSpaceForm = memo(function AddSpaceForm({
  canSubmit,
  creating,
  onSubmit,
  resetKey,
}: AddSpaceFormProps) {
  const [spaceName, setSpaceName] = useState('');
  const [spaceDescription, setSpaceDescription] = useState('');
  const canCreateSpace = canSubmit && Boolean(spaceName.trim()) && !creating;

  useEffect(() => {
    setSpaceName('');
    setSpaceDescription('');
  }, [resetKey]);

  const submit = useCallback(() => {
    onSubmit(spaceName, spaceDescription);
  }, [onSubmit, spaceDescription, spaceName]);

  return (
    <View style={styles.formPanel}>
      <View style={styles.formHeader}>
        <MaterialIcons color="#526049" name="create-new-folder" size={22} />
        <Text style={styles.formTitle}>Add Space</Text>
      </View>

      <View style={styles.field}>
        <Text style={styles.label}>Name</Text>
        <TextInput
          autoCapitalize="words"
          onChangeText={setSpaceName}
          onSubmitEditing={submit}
          placeholder="Kitchen"
          placeholderTextColor="#8c8a81"
          returnKeyType="next"
          style={styles.input}
          value={spaceName}
        />
      </View>

      <View style={styles.field}>
        <Text style={styles.label}>Description</Text>
        <TextInput
          multiline
          onChangeText={setSpaceDescription}
          placeholder="Cooking tools and everyday food storage"
          placeholderTextColor="#8c8a81"
          style={[styles.input, styles.textArea]}
          value={spaceDescription}
        />
      </View>

      <Pressable
        accessibilityRole="button"
        disabled={!canCreateSpace}
        onPress={submit}
        style={({ pressed }) => [
          styles.primaryButton,
          pressed && styles.primaryButtonPressed,
          !canCreateSpace && styles.primaryButtonDisabled,
        ]}>
        {creating ? (
          <ActivityIndicator color="#ffffff" />
        ) : (
          <>
            <MaterialIcons color="#ffffff" name="add" size={20} />
            <Text style={styles.primaryButtonText}>Create space</Text>
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
  textArea: {
    minHeight: 86,
    paddingTop: 14,
    textAlignVertical: 'top',
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
