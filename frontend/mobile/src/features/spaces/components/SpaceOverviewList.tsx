import type { ReactElement } from 'react';
import { FlatList, RefreshControl, StyleSheet, View } from 'react-native';

import type { ItemView } from '@/features/items/api';
import type { SpaceView } from '@/features/spaces/api';
import { EmptySpaceItems, EmptySubSpaces } from '@/features/spaces/components/SpaceOverviewEmptyStates';
import { SpaceItemListItem } from '@/features/spaces/components/SpaceItemListItem';
import { SpaceListItem } from '@/features/spaces/components/SpaceListItem';
import type { OverviewTab } from '@/features/spaces/components/OverviewTabs';

export type OverviewListItem = SpaceView | ItemView;

type SpaceOverviewListProps = {
  activeTab: OverviewTab;
  data: OverviewListItem[];
  error: string | null;
  hasSelectedSpace: boolean;
  listHeader: ReactElement;
  loading: boolean;
  addingItemToShoppingListId?: string | null;
  onAddItem: () => void;
  onAddToShoppingList?: (item: ItemView) => void;
  onCreateSubSpace: () => void;
  onItemPress: (item: ItemView) => void;
  onRefresh: () => void;
  onScan: () => void;
  onSpacePress: (space: SpaceView) => void;
  refreshing: boolean;
};

export function SpaceOverviewList({
  activeTab,
  data,
  error,
  hasSelectedSpace,
  listHeader,
  loading,
  addingItemToShoppingListId,
  onAddItem,
  onAddToShoppingList,
  onCreateSubSpace,
  onItemPress,
  onRefresh,
  onScan,
  onSpacePress,
  refreshing,
}: SpaceOverviewListProps) {
  return (
    <FlatList<OverviewListItem>
      ListHeaderComponent={listHeader}
      ListHeaderComponentStyle={styles.listHeader}
      contentContainerStyle={styles.listContent}
      data={data}
      ItemSeparatorComponent={() => <View style={styles.separator} />}
      keyExtractor={(item) => item.id}
      keyboardShouldPersistTaps="handled"
      ListEmptyComponent={
        !loading && !error ? (
          activeTab === 'subSpaces' ? (
            <EmptySubSpaces onCreate={onCreateSubSpace} />
          ) : (
            <EmptySpaceItems
              hasSelectedSpace={hasSelectedSpace}
              onCreate={onAddItem}
              onScan={onScan}
            />
          )
        ) : null
      }
      refreshControl={
        <RefreshControl refreshing={refreshing} onRefresh={onRefresh} tintColor="#526049" />
      }
      renderItem={({ item }) =>
        activeTab === 'subSpaces' ? (
          <SpaceListItem onPress={onSpacePress} space={item as SpaceView} />
        ) : hasSelectedSpace ? (
          <SpaceItemListItem
            addingToShoppingList={addingItemToShoppingListId === item.id}
            item={item as ItemView}
            onAddToShoppingList={onAddToShoppingList}
            onPress={onItemPress}
          />
        ) : null
      }
    />
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
});
