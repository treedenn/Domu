import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { StyleSheet, Text, View } from 'react-native';

export function ScannerTipPanel() {
  return (
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
  );
}

const styles = StyleSheet.create({
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
});
