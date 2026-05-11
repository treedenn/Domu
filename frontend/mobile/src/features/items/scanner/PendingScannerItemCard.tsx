import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { useCallback } from 'react';
import { Image, Pressable, StyleSheet, Text, TextInput, View } from 'react-native';

import type { PendingScannerItem } from '@/features/items/scanner/scannerBasketModel';

type PendingScannerItemCardProps = {
  index: number;
  item: PendingScannerItem;
  onChange: (index: number, item: PendingScannerItem) => void;
  onRemove: (index: number) => void;
};

export function PendingScannerItemCard({
  index,
  item,
  onChange,
  onRemove,
}: PendingScannerItemCardProps) {
  const setName = useCallback(
    (name: string) => {
      onChange(index, { ...item, name });
    },
    [index, item, onChange],
  );

  const setCategory = useCallback(
    (category: string) => {
      onChange(index, { ...item, category });
    },
    [index, item, onChange],
  );

  const setBarcode = useCallback(
    (barcode: string) => {
      onChange(index, { ...item, barcode: barcode.trim() || null });
    },
    [index, item, onChange],
  );

  const setQuantity = useCallback(
    (quantity: string) => {
      onChange(index, { ...item, quantity: Math.max(Number(quantity) || 1, 1) });
    },
    [index, item, onChange],
  );

  return (
    <View style={styles.itemCard}>
      <View style={styles.itemVisual}>
        {item.imageUri ? (
          <Image source={{ uri: item.imageUri }} style={styles.itemImage} />
        ) : (
          <MaterialIcons
            color={item.source === 'photo' ? '#494740' : '#526049'}
            name={item.source === 'photo' ? 'photo-library' : 'qr-code-scanner'}
            size={30}
          />
        )}
      </View>
      <View style={styles.itemContent}>
        <View style={styles.itemTopRow}>
          <Text style={styles.itemSource}>
            {item.source === 'photo' ? 'Photo scan' : 'Barcode scan'}
          </Text>
          <Pressable
            accessibilityLabel="Remove item"
            accessibilityRole="button"
            onPress={() => onRemove(index)}
            style={({ pressed }) => [styles.smallIconButton, pressed && styles.pressed]}>
            <MaterialIcons color="#757870" name="close" size={18} />
          </Pressable>
        </View>

        <TextInput
          autoCapitalize="words"
          onChangeText={setName}
          placeholder="Item name"
          placeholderTextColor="#8c8a81"
          style={styles.cardInput}
          value={item.name}
        />
        <View style={styles.cardInputRow}>
          <TextInput
            onChangeText={setCategory}
            placeholder="Category"
            placeholderTextColor="#8c8a81"
            style={[styles.cardInput, styles.cardInputHalf]}
            value={item.category}
          />
          <TextInput
            keyboardType="number-pad"
            onChangeText={setQuantity}
            placeholder="Qty"
            placeholderTextColor="#8c8a81"
            style={[styles.cardInput, styles.quantityCardInput]}
            value={String(item.quantity)}
          />
        </View>
        <TextInput
          autoCapitalize="none"
          keyboardType="number-pad"
          onChangeText={setBarcode}
          placeholder="Barcode"
          placeholderTextColor="#8c8a81"
          style={styles.cardInput}
          value={item.barcode ?? ''}
        />
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  itemCard: {
    backgroundColor: '#ffffff',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    gap: 14,
    padding: 14,
    shadowColor: '#5c5854',
    shadowOffset: { height: 4, width: 0 },
    shadowOpacity: 0.05,
    shadowRadius: 12,
  },
  itemVisual: {
    alignItems: 'center',
    backgroundColor: '#f4ede7',
    borderRadius: 8,
    height: 86,
    justifyContent: 'center',
    overflow: 'hidden',
    width: 86,
  },
  itemImage: {
    height: '100%',
    width: '100%',
  },
  itemContent: {
    flex: 1,
    gap: 8,
    minWidth: 0,
  },
  itemTopRow: {
    alignItems: 'center',
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  itemSource: {
    color: '#757870',
    fontSize: 12,
    fontWeight: '700',
  },
  smallIconButton: {
    alignItems: 'center',
    height: 28,
    justifyContent: 'center',
    width: 28,
  },
  cardInput: {
    backgroundColor: '#faf2ed',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    color: '#1e1b18',
    fontSize: 14,
    minHeight: 42,
    paddingHorizontal: 10,
  },
  cardInputRow: {
    flexDirection: 'row',
    gap: 8,
  },
  cardInputHalf: {
    flex: 1,
  },
  quantityCardInput: {
    textAlign: 'center',
    width: 64,
  },
  pressed: {
    opacity: 0.78,
  },
});
