import { Pressable, StyleSheet, Text, View } from 'react-native';

export type OverviewTab = 'subSpaces' | 'items';

type OverviewTabsProps = {
  activeTab: OverviewTab;
  onChange: (tab: OverviewTab) => void;
};

export function OverviewTabs({ activeTab, onChange }: OverviewTabsProps) {
  return (
    <View style={styles.tabs}>
      <TabButton
        active={activeTab === 'subSpaces'}
        label="Sub-spaces"
        onPress={() => onChange('subSpaces')}
      />
      <TabButton
        active={activeTab === 'items'}
        label="Items"
        onPress={() => onChange('items')}
      />
    </View>
  );
}

function TabButton({
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
      accessibilityRole="tab"
      accessibilityState={{ selected: active }}
      onPress={onPress}
      style={({ pressed }) => [
        styles.tabButton,
        active && styles.tabButtonActive,
        pressed && styles.pressed,
      ]}>
      <Text style={[styles.tabButtonText, active && styles.tabButtonTextActive]}>{label}</Text>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  tabs: {
    backgroundColor: '#faf2ed',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    gap: 4,
    padding: 4,
  },
  tabButton: {
    alignItems: 'center',
    borderRadius: 8,
    flex: 1,
    justifyContent: 'center',
    minHeight: 42,
    paddingHorizontal: 12,
  },
  tabButtonActive: {
    backgroundColor: '#526049',
  },
  tabButtonText: {
    color: '#444841',
    fontSize: 14,
    fontWeight: '800',
    letterSpacing: 0,
  },
  tabButtonTextActive: {
    color: '#ffffff',
  },
  pressed: {
    opacity: 0.78,
  },
});
