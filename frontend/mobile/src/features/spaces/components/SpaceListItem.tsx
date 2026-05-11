import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import type { ComponentProps } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';

import type { SpaceView } from '@/features/spaces/api';

type SpaceListItemProps = {
  onPress: (space: SpaceView) => void;
  space: SpaceView;
};

export function SpaceListItem({ onPress, space }: SpaceListItemProps) {
  const itemCount = space.items?.count ?? 0;
  const childCount = space.childSpaces?.count ?? 0;

  return (
    <Pressable
      accessibilityRole="button"
      onPress={() => onPress(space)}
      style={({ pressed }) => [styles.spaceCard, pressed && styles.cardPressed]}>
      <View style={styles.spaceIcon}>
        <MaterialIcons color="#526049" name="inventory-2" size={24} />
      </View>

      <View style={styles.spaceContent}>
        <Text numberOfLines={1} style={styles.spaceName}>
          {space.name}
        </Text>
        {space.description ? (
          <Text numberOfLines={2} style={styles.spaceDescription}>
            {space.description}
          </Text>
        ) : null}
        <View style={styles.spaceMetaRow}>
          <SpaceMeta icon="category" label={formatCount(childCount, 'space')} />
          <SpaceMeta icon="kitchen" label={formatCount(itemCount, 'item')} />
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

const styles = StyleSheet.create({
  spaceCard: {
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
  spaceIcon: {
    alignItems: 'center',
    backgroundColor: '#d8e8cb',
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
