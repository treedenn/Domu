import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { useState, type ComponentProps } from 'react';
import {
  ActivityIndicator,
  Modal,
  Pressable,
  StyleSheet,
  Text,
  View,
} from 'react-native';

export type AppTopBarAction = {
  destructive?: boolean;
  disabled?: boolean;
  icon: ComponentProps<typeof MaterialIcons>['name'];
  label: string;
  loading?: boolean;
  onPress: () => void;
};

export function AppTopBar({
  actions = [],
  backAccessibilityLabel = 'Go back',
  onBack,
  subtitle,
  title,
}: {
  actions?: AppTopBarAction[];
  backAccessibilityLabel?: string;
  onBack?: () => void;
  subtitle?: string | null;
  title: string;
}) {
  const [menuVisible, setMenuVisible] = useState(false);
  const hasActions = actions.length > 0;

  const selectAction = (action: AppTopBarAction) => {
    if (action.disabled || action.loading) {
      return;
    }

    setMenuVisible(false);
    action.onPress();
  };

  return (
    <View style={styles.topBar}>
      {onBack ? (
        <Pressable
          accessibilityLabel={backAccessibilityLabel}
          accessibilityRole="button"
          onPress={onBack}
          style={({ pressed }) => [styles.iconButton, pressed && styles.pressed]}>
          <MaterialIcons color="#526049" name="arrow-back" size={22} />
        </Pressable>
      ) : (
        <View style={styles.iconSlot} />
      )}

      <View style={styles.titleBlock}>
        <Text numberOfLines={1} style={styles.title}>
          {title}
        </Text>
        {subtitle ? (
          <Text numberOfLines={1} style={styles.subtitle}>
            {subtitle}
          </Text>
        ) : null}
      </View>

      {hasActions ? (
        <>
          <Pressable
            accessibilityLabel="Open actions"
            accessibilityRole="button"
            onPress={() => setMenuVisible(true)}
            style={({ pressed }) => [styles.iconButton, pressed && styles.pressed]}>
            <MaterialIcons color="#526049" name="more-vert" size={22} />
          </Pressable>
          <Modal
            animationType="fade"
            transparent
            visible={menuVisible}
            onRequestClose={() => setMenuVisible(false)}>
            <Pressable style={styles.menuBackdrop} onPress={() => setMenuVisible(false)}>
              <View style={styles.menu}>
                {actions.map((action, index) => (
                  <Pressable
                    accessibilityRole="button"
                    disabled={action.disabled || action.loading}
                    key={`${action.label}-${index}`}
                    onPress={() => selectAction(action)}
                    style={({ pressed }) => [
                      styles.menuItem,
                      action.destructive && styles.menuItemDestructive,
                      (action.disabled || action.loading) && styles.menuItemDisabled,
                      pressed && styles.pressed,
                    ]}>
                    {action.loading ? (
                      <ActivityIndicator
                        color={action.destructive ? '#944931' : '#526049'}
                        size="small"
                      />
                    ) : (
                      <MaterialIcons
                        color={action.destructive ? '#944931' : '#526049'}
                        name={action.icon}
                        size={20}
                      />
                    )}
                    <Text
                      style={[
                        styles.menuItemText,
                        action.destructive && styles.menuItemTextDestructive,
                      ]}>
                      {action.label}
                    </Text>
                  </Pressable>
                ))}
              </View>
            </Pressable>
          </Modal>
        </>
      ) : (
        <View style={styles.iconSlot} />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  topBar: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 10,
    justifyContent: 'space-between',
  },
  iconButton: {
    alignItems: 'center',
    backgroundColor: '#faf2ed',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    height: 44,
    justifyContent: 'center',
    width: 44,
  },
  iconSlot: {
    height: 44,
    width: 44,
  },
  titleBlock: {
    alignItems: 'center',
    flex: 1,
    gap: 2,
    minWidth: 0,
  },
  title: {
    color: '#1e1b18',
    fontSize: 16,
    fontWeight: '800',
    letterSpacing: 0,
  },
  subtitle: {
    color: '#757870',
    fontSize: 12,
    fontWeight: '700',
  },
  menuBackdrop: {
    alignItems: 'flex-end',
    backgroundColor: 'rgba(30, 27, 24, 0.18)',
    flex: 1,
    paddingHorizontal: 20,
    paddingTop: 64,
  },
  menu: {
    backgroundColor: '#ffffff',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    minWidth: 210,
    overflow: 'hidden',
    shadowColor: '#5c5854',
    shadowOffset: { height: 8, width: 0 },
    shadowOpacity: 0.08,
    shadowRadius: 24,
  },
  menuItem: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 10,
    minHeight: 48,
    paddingHorizontal: 14,
  },
  menuItemDestructive: {
    backgroundColor: '#fff5f1',
  },
  menuItemDisabled: {
    opacity: 0.5,
  },
  menuItemText: {
    color: '#1e1b18',
    fontSize: 14,
    fontWeight: '800',
  },
  menuItemTextDestructive: {
    color: '#944931',
  },
  pressed: {
    opacity: 0.78,
  },
});
