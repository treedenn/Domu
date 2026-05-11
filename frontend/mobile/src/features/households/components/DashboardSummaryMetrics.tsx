import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { StyleSheet, Text, View } from 'react-native';

type DashboardSummaryMetricsProps = {
  itemCount: number;
  totalSpaces: number;
};

export function DashboardSummaryMetrics({
  itemCount,
  totalSpaces,
}: DashboardSummaryMetricsProps) {
  return (
    <View style={styles.summaryGrid}>
      <SummaryMetric icon="inventory-2" label="Spaces" value={totalSpaces} />
      <SummaryMetric icon="kitchen" label="Items" value={itemCount} />
    </View>
  );
}

function SummaryMetric({
  icon,
  label,
  value,
}: {
  icon: React.ComponentProps<typeof MaterialIcons>['name'];
  label: string;
  value: number;
}) {
  return (
    <View style={styles.summaryMetric}>
      <MaterialIcons color="#526049" name={icon} size={22} />
      <Text style={styles.summaryValue}>{value}</Text>
      <Text style={styles.summaryLabel}>{label}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  summaryGrid: {
    flexDirection: 'row',
    gap: 10,
  },
  summaryMetric: {
    backgroundColor: '#ffffff',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    flex: 1,
    gap: 6,
    minHeight: 104,
    padding: 16,
  },
  summaryValue: {
    color: '#1e1b18',
    fontSize: 28,
    fontWeight: '800',
    letterSpacing: 0,
  },
  summaryLabel: {
    color: '#444841',
    fontSize: 13,
    fontWeight: '700',
  },
});
