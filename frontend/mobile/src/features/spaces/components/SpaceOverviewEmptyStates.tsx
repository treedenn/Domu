import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { Pressable, StyleSheet, Text, View } from 'react-native';

export function EmptySubSpaces({ onCreate }: { onCreate: () => void }) {
  return (
    <View style={styles.emptyPanel}>
      <View style={styles.emptyIcon}>
        <MaterialIcons color="#526049" name="inventory-2" size={28} />
      </View>
      <Text style={styles.emptyTitle}>No sub-spaces here yet</Text>
      <Text style={styles.emptyText}>Add a sub-space to organize this part of the home.</Text>
      <Pressable
        accessibilityRole="button"
        onPress={onCreate}
        style={({ pressed }) => [styles.emptyButton, pressed && styles.pressed]}>
        <Text style={styles.emptyButtonText}>Create sub-space</Text>
      </Pressable>
    </View>
  );
}

export function EmptySpaceItems({
  hasSelectedSpace,
  onCreate,
  onScan,
}: {
  hasSelectedSpace: boolean;
  onCreate: () => void;
  onScan: () => void;
}) {
  return (
    <View style={styles.emptyPanel}>
      <View style={styles.emptyIcon}>
        <MaterialIcons color="#944931" name="kitchen" size={28} />
      </View>
      <Text style={styles.emptyTitle}>
        {hasSelectedSpace ? 'No items here yet' : 'Open a space first'}
      </Text>
      <Text style={styles.emptyText}>
        {hasSelectedSpace
          ? 'Items added to this space will appear here.'
          : 'Household-level item browsing needs a selected space.'}
      </Text>
      {hasSelectedSpace ? (
        <View style={styles.emptyActionRow}>
          <Pressable
            accessibilityRole="button"
            onPress={onCreate}
            style={({ pressed }) => [
              styles.emptyButton,
              styles.emptyActionButton,
              pressed && styles.pressed,
            ]}>
            <Text style={styles.emptyButtonText}>Create item</Text>
          </Pressable>
          <Pressable
            accessibilityRole="button"
            onPress={onScan}
            style={({ pressed }) => [styles.emptySecondaryButton, pressed && styles.pressed]}>
            <MaterialIcons color="#526049" name="qr-code-scanner" size={18} />
            <Text style={styles.emptySecondaryButtonText}>Scan</Text>
          </Pressable>
        </View>
      ) : null}
    </View>
  );
}

const styles = StyleSheet.create({
  emptyPanel: {
    alignItems: 'center',
    backgroundColor: '#faf2ed',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    gap: 8,
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
  emptyButton: {
    alignItems: 'center',
    backgroundColor: '#526049',
    borderRadius: 8,
    flex: 1,
    justifyContent: 'center',
    marginTop: 8,
    minHeight: 42,
    paddingHorizontal: 16,
  },
  emptyButtonText: {
    color: '#ffffff',
    fontSize: 14,
    fontWeight: '800',
  },
  emptyActionRow: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 10,
    marginTop: 8,
    width: '100%',
  },
  emptyActionButton: {
    marginTop: 0,
  },
  emptySecondaryButton: {
    alignItems: 'center',
    backgroundColor: '#ffffff',
    borderColor: '#526049',
    borderRadius: 8,
    borderWidth: 1,
    flex: 1,
    flexDirection: 'row',
    gap: 6,
    justifyContent: 'center',
    minHeight: 42,
    paddingHorizontal: 14,
  },
  emptySecondaryButtonText: {
    color: '#526049',
    fontSize: 14,
    fontWeight: '800',
  },
  pressed: {
    opacity: 0.78,
  },
});
