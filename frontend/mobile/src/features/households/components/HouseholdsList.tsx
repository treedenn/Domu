import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import type { ReactElement } from 'react';
import { FlatList, RefreshControl, StyleSheet, Text, View } from 'react-native';

import type { HouseholdView } from '@/features/households/api';
import { HouseholdsListItem } from '@/features/households/components/HouseholdsListItem';

type HouseholdsListProps = {
  error: string | null;
  households: HouseholdView[];
  listHeader: ReactElement;
  loading: boolean;
  onHouseholdPress: (household: HouseholdView) => void;
  onRefresh: () => void;
  refreshing: boolean;
};

export function HouseholdsList({
  error,
  households,
  listHeader,
  loading,
  onHouseholdPress,
  onRefresh,
  refreshing,
}: HouseholdsListProps) {
  return (
    <FlatList
      ListHeaderComponent={listHeader}
      contentContainerStyle={styles.listContent}
      data={households}
      keyExtractor={(item) => item.id}
      keyboardShouldPersistTaps="handled"
      ListEmptyComponent={!loading && !error ? <EmptyHouseholds /> : null}
      refreshControl={
        <RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor="#526049" />
      }
      renderItem={({ item }) => (
        <HouseholdsListItem household={item} onPress={onHouseholdPress} />
      )}
      ItemSeparatorComponent={() => <View style={styles.separator} />}
    />
  );
}

function EmptyHouseholds() {
  return (
    <View style={styles.emptyPanel}>
      <View style={styles.emptyIcon}>
        <MaterialIcons color="#526049" name="home-work" size={28} />
      </View>
      <Text style={styles.emptyTitle}>No households yet</Text>
      <Text style={styles.emptyText}>Create your first household to start organizing spaces.</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  listContent: {
    paddingBottom: 32,
    paddingHorizontal: 20,
  },
  separator: {
    height: 12,
  },
  emptyPanel: {
    alignItems: 'center',
    backgroundColor: '#faf2ed',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    gap: 8,
    marginTop: 12,
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
});
