import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { Pressable, StyleSheet, Text, View } from 'react-native';

import type { SpaceView } from '@/features/spaces/api';

type SpacesListItemProps = {
  onPress: (space: SpaceView) => void;
  space: SpaceView;
};

export function SpacesListItem({ onPress, space }: SpacesListItemProps) {
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
        <Text style={styles.spaceDetails}>
          {formatCount(space.childSpaces?.count ?? 0, 'space')} -{' '}
          {formatCount(space.items?.count ?? 0, 'item')}
        </Text>
      </View>
      <MaterialIcons color="#757870" name="chevron-right" size={24} />
    </Pressable>
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
    minHeight: 78,
    padding: 14,
  },
  cardPressed: {
    backgroundColor: '#faf2ed',
  },
  spaceIcon: {
    alignItems: 'center',
    backgroundColor: '#d8e8cb',
    borderRadius: 8,
    height: 46,
    justifyContent: 'center',
    width: 46,
  },
  spaceContent: {
    flex: 1,
    gap: 4,
    minWidth: 0,
  },
  spaceName: {
    color: '#1e1b18',
    fontSize: 17,
    fontWeight: '800',
    letterSpacing: 0,
  },
  spaceDetails: {
    color: '#444841',
    fontSize: 13,
    lineHeight: 18,
  },
});
