import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import type { ReactElement } from 'react';
import {
  FlatList,
  Pressable,
  RefreshControl,
  StyleSheet,
  Text,
  View,
} from 'react-native';

import type { SpaceView } from '@/features/spaces/api';
import { SpacesListItem } from '@/features/households/components/SpacesListItem';

type SpacesListProps = {
  error: string | null;
  listHeader: ReactElement;
  loading: boolean;
  onCreate: () => void;
  onRefresh: () => void;
  onSpacePress: (space: SpaceView) => void;
  refreshing: boolean;
  spaces: SpaceView[];
};

export function SpacesList({
  error,
  listHeader,
  loading,
  onCreate,
  onRefresh,
  onSpacePress,
  refreshing,
  spaces,
}: SpacesListProps) {
  return (
    <FlatList
      ListHeaderComponent={listHeader}
      ListHeaderComponentStyle={styles.listHeader}
      contentContainerStyle={styles.listContent}
      data={spaces}
      ItemSeparatorComponent={() => <View style={styles.separator} />}
      keyExtractor={(item) => item.id}
      ListEmptyComponent={!loading && !error ? <EmptySpaces onCreate={onCreate} /> : null}
      refreshControl={
        <RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor="#526049" />
      }
      renderItem={({ item }) => <SpacesListItem onPress={onSpacePress} space={item} />}
    />
  );
}

function EmptySpaces({ onCreate }: { onCreate: () => void }) {
  return (
    <View style={styles.emptyPanel}>
      <View style={styles.emptyIcon}>
        <MaterialIcons color="#526049" name="inventory-2" size={28} />
      </View>
      <Text style={styles.emptyTitle}>No spaces yet</Text>
      <Text style={styles.emptyText}>Create the first storage area for this household.</Text>
      <Pressable
        accessibilityRole="button"
        onPress={onCreate}
        style={({ pressed }) => [styles.emptyButton, pressed && styles.pressed]}>
        <Text style={styles.emptyButtonText}>Create space</Text>
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  listContent: {
    paddingBottom: 32,
    paddingHorizontal: 20,
  },
  listHeader: {
    marginBottom: 12,
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
  pressed: {
    opacity: 0.78,
  },
});
