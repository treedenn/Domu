import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { type ComponentProps } from 'react';
import { ActivityIndicator, Pressable, StyleSheet, Text, View } from 'react-native';

export function MethodTile({
  icon,
  label,
  loading,
  meta,
  onPress,
  tone,
}: {
  icon: ComponentProps<typeof MaterialIcons>['name'];
  label: string;
  loading?: boolean;
  meta: string;
  onPress: () => void;
  tone: 'primary' | 'tertiary';
}) {
  return (
    <Pressable
      accessibilityRole="button"
      onPress={onPress}
      style={({ pressed }) => [styles.methodTile, pressed && styles.pressed]}>
      <View style={[styles.methodIcon, tone === 'tertiary' && styles.methodIconTertiary]}>
        {loading ? (
          <ActivityIndicator color={tone === 'tertiary' ? '#494740' : '#f8ffee'} />
        ) : (
          <MaterialIcons
            color={tone === 'tertiary' ? '#494740' : '#f8ffee'}
            name={icon}
            size={34}
          />
        )}
      </View>
      <Text style={styles.methodTitle}>{label}</Text>
      <Text style={styles.methodMeta}>{meta}</Text>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  methodTile: {
    alignItems: 'center',
    backgroundColor: '#ffffff',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    gap: 8,
    justifyContent: 'center',
    minHeight: 180,
    padding: 24,
    shadowColor: '#5c5854',
    shadowOffset: { height: 4, width: 0 },
    shadowOpacity: 0.05,
    shadowRadius: 12,
  },
  methodIcon: {
    alignItems: 'center',
    backgroundColor: '#6a7961',
    borderRadius: 999,
    height: 64,
    justifyContent: 'center',
    marginBottom: 8,
    width: 64,
  },
  methodIconTertiary: {
    backgroundColor: '#e7e2d9',
  },
  methodTitle: {
    color: '#1e1b18',
    fontSize: 20,
    fontWeight: '700',
  },
  methodMeta: {
    color: '#444841',
    fontSize: 13,
    fontWeight: '600',
    textAlign: 'center',
  },
  pressed: {
    opacity: 0.78,
  },
});
