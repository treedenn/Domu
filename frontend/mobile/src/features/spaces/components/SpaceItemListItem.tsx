import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import type { ComponentProps } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';

import type { ItemView } from '@/features/items/api';

type SpaceItemListItemProps = {
  item: ItemView;
  onPress: (item: ItemView) => void;
};

export function SpaceItemListItem({ item, onPress }: SpaceItemListItemProps) {
  return (
    <Pressable
      accessibilityRole="button"
      onPress={() => onPress(item)}
      style={({ pressed }) => [styles.itemCard, pressed && styles.cardPressed]}>
      <View style={styles.itemIcon}>
        <MaterialIcons color="#944931" name="kitchen" size={24} />
      </View>

      <View style={styles.spaceContent}>
        <Text numberOfLines={1} style={styles.spaceName}>
          {item.name}
        </Text>
        <Text style={styles.spaceDescription}>
          {item.category || 'Uncategorized'} - {formatQuantity(item.totalQuantity)}
        </Text>
        <View style={styles.spaceMetaRow}>
          <SpaceMeta icon="inventory" label={formatCount(item.entries.length, 'entry')} />
          {item.barcode ? <SpaceMeta icon="qr-code-2" label={item.barcode} /> : null}
        </View>
      </View>

      <MaterialIcons color="#757870" name="chevron-right" size={24} />
    </Pressable>
  );
}

function SpaceMeta({
  icon,
  label,
}: {
  icon: ComponentProps<typeof MaterialIcons>['name'];
  label: string;
}) {
  return (
    <View style={styles.spaceMeta}>
      <MaterialIcons color="#757870" name={icon} size={15} />
      <Text style={styles.spaceMetaText}>{label}</Text>
    </View>
  );
}

function formatCount(value: number, noun: string) {
  return `${value} ${noun}${value === 1 ? '' : 's'}`;
}

function formatQuantity(value: number) {
  return `${value.toLocaleString()} total`;
}

const styles = StyleSheet.create({
  itemCard: {
    alignItems: 'center',
    backgroundColor: '#ffffff',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    gap: 12,
    minHeight: 92,
    padding: 14,
  },
  cardPressed: {
    backgroundColor: '#faf2ed',
  },
  itemIcon: {
    alignItems: 'center',
    backgroundColor: '#ffdbd0',
    borderRadius: 8,
    height: 48,
    justifyContent: 'center',
    width: 48,
  },
  spaceContent: {
    flex: 1,
    gap: 6,
    minWidth: 0,
  },
  spaceName: {
    color: '#1e1b18',
    fontSize: 17,
    fontWeight: '800',
    letterSpacing: 0,
  },
  spaceDescription: {
    color: '#444841',
    fontSize: 13,
    lineHeight: 18,
  },
  spaceMetaRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  spaceMeta: {
    alignItems: 'center',
    backgroundColor: '#faf2ed',
    borderRadius: 8,
    flexDirection: 'row',
    gap: 4,
    minHeight: 26,
    paddingHorizontal: 8,
  },
  spaceMetaText: {
    color: '#444841',
    fontSize: 12,
    fontWeight: '700',
  },
});
