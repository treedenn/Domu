import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { Pressable, StyleSheet, Text, View } from 'react-native';

import {
  HouseholdSubscriptionPlan,
  HouseholdSubscriptionStatus,
  type HouseholdView,
} from '@/features/households/api';

type HouseholdsListItemProps = {
  household: HouseholdView;
  onPress: (household: HouseholdView) => void;
};

export function HouseholdsListItem({ household, onPress }: HouseholdsListItemProps) {
  return (
    <Pressable
      accessibilityRole="button"
      onPress={() => onPress(household)}
      style={({ pressed }) => [styles.householdCard, pressed && styles.cardPressed]}>
      <View style={styles.householdIcon}>
        <MaterialIcons color="#526049" name="home" size={24} />
      </View>
      <View style={styles.householdContent}>
        <Text numberOfLines={1} style={styles.householdName}>
          {household.name}
        </Text>
        <Text style={styles.householdDetails}>{formatSubscription(household)}</Text>
      </View>
      <MaterialIcons color="#757870" name="chevron-right" size={24} />
    </Pressable>
  );
}

function formatSubscription(household: HouseholdView) {
  const plan = household.subscriptionPlan === HouseholdSubscriptionPlan.Premium ? 'Premium' : 'Free';
  const status =
    household.subscriptionStatus === HouseholdSubscriptionStatus.CancellationScheduled
      ? 'Cancellation scheduled'
      : 'Active';

  return `${plan} plan - ${status}`;
}

const styles = StyleSheet.create({
  householdCard: {
    alignItems: 'center',
    backgroundColor: '#ffffff',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    gap: 12,
    minHeight: 78,
    padding: 14,
  },
  cardPressed: {
    backgroundColor: '#faf2ed',
  },
  householdIcon: {
    alignItems: 'center',
    backgroundColor: '#d8e8cb',
    borderRadius: 8,
    height: 46,
    justifyContent: 'center',
    width: 46,
  },
  householdContent: {
    flex: 1,
    gap: 4,
    minWidth: 0,
  },
  householdName: {
    color: '#1e1b18',
    fontSize: 17,
    fontWeight: '800',
    letterSpacing: 0,
  },
  householdDetails: {
    color: '#444841',
    fontSize: 13,
    lineHeight: 18,
  },
});
