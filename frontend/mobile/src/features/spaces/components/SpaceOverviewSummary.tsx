import { StyleSheet, Text, View } from 'react-native';

type SpaceOverviewSummaryProps = {
  itemCount: number;
  spaceCount: number;
};

export function SpaceOverviewSummary({ itemCount, spaceCount }: SpaceOverviewSummaryProps) {
  return (
    <View style={styles.summaryGrid}>
      <SummaryMetric label="Spaces" value={spaceCount} />
      <SummaryMetric label="Items" value={itemCount} />
    </View>
  );
}

function SummaryMetric({ label, value }: { label: string; value: number }) {
  return (
    <View style={styles.summaryMetric}>
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
    backgroundColor: '#faf2ed',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    flex: 1,
    gap: 4,
    minHeight: 78,
    padding: 14,
  },
  summaryValue: {
    color: '#1e1b18',
    fontSize: 24,
    fontWeight: '800',
    letterSpacing: 0,
  },
  summaryLabel: {
    color: '#444841',
    fontSize: 13,
    fontWeight: '700',
  },
});
