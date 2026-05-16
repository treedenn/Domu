import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { useCallback, useEffect, useState } from 'react';
import {
  ActivityIndicator,
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';

import {
  ConsumableState,
  ItemContainerType,
  ItemUnit,
  type ItemEntryView,
} from '@/features/items/api';

type ItemEntryRowProps = {
  deleting: boolean;
  entry: ItemEntryView;
  index: number;
  onDelete: (entry: ItemEntryView) => void;
  onEdit: () => void;
  onAddToShoppingList?: (entry: ItemEntryView) => void;
  onOpen: (entry: ItemEntryView) => void;
  onSetCurrentQuantity: (entry: ItemEntryView, currentQuantity: number) => void;
  updating: boolean;
};

const fractionOptions = [
  { label: 'Full', value: 1 },
  { label: '3/4', value: 0.75 },
  { label: '1/2', value: 0.5 },
  { label: '1/4', value: 0.25 },
  { label: 'Empty', value: 0 },
];

export function ItemEntryRow({
  deleting,
  entry,
  index,
  onDelete,
  onEdit,
  onAddToShoppingList,
  onOpen,
  onSetCurrentQuantity,
  updating,
}: ItemEntryRowProps) {
  const [expanded, setExpanded] = useState(false);
  const [customQuantity, setCustomQuantity] = useState(formatInputQuantity(entry.currentQuantity));
  const busy = deleting || updating;
  const opened = entry.state === ConsumableState.Opened;
  const unopened = entry.state === ConsumableState.Unopened;
  const canDecrease = opened && entry.currentQuantity > 0;
  const canIncrease = opened && entry.currentQuantity < entry.initialQuantity;
  const showSteppers = opened && entry.unit === ItemUnit.Piece;
  const showFractions = opened && entry.unit !== ItemUnit.Piece;
  const unitLabel = formatUnit(entry.unit);

  useEffect(() => {
    setCustomQuantity(formatInputQuantity(entry.currentQuantity));
  }, [entry.currentQuantity]);

  const setFraction = useCallback(
    (fraction: number) => {
      onSetCurrentQuantity(entry, roundQuantity(entry.initialQuantity * fraction));
    },
    [entry, onSetCurrentQuantity],
  );

  const applyCustomQuantity = useCallback(() => {
    onSetCurrentQuantity(entry, parseQuantityInput(customQuantity));
    setExpanded(false);
  }, [customQuantity, entry, onSetCurrentQuantity]);

  return (
    <View style={styles.entryRow}>
      <View style={styles.entryNumber}>
        <Text style={styles.entryNumberText}>{index + 1}</Text>
      </View>
      <View style={styles.entryContent}>
        <View style={styles.entryTopRow}>
          <Text style={styles.entryTitle}>{formatEntryQuantity(entry)}</Text>
          <View style={styles.entryActions}>
            <Text style={styles.entryState}>{formatState(entry.state)}</Text>
            <Pressable
              accessibilityLabel="Edit entry"
              accessibilityRole="button"
              disabled={busy}
              onPress={onEdit}
              style={({ pressed }) => [
                styles.entryEditButton,
                pressed && styles.pressed,
                busy && styles.disabledButton,
              ]}>
              <MaterialIcons color="#526049" name="edit" size={18} />
            </Pressable>
            {onAddToShoppingList ? (
              <Pressable
                accessibilityLabel="Add entry to shopping list"
                accessibilityRole="button"
                disabled={busy}
                onPress={() => onAddToShoppingList(entry)}
                style={({ pressed }) => [
                  styles.entryEditButton,
                  pressed && styles.pressed,
                  busy && styles.disabledButton,
                ]}>
                <MaterialIcons color="#526049" name="shopping-bag" size={18} />
              </Pressable>
            ) : null}
            <Pressable
              accessibilityLabel="Delete entry"
              accessibilityRole="button"
              disabled={busy}
              onPress={() => onDelete(entry)}
              style={({ pressed }) => [
                styles.entryDeleteButton,
                pressed && styles.pressed,
                busy && styles.disabledButton,
              ]}>
              {deleting ? (
                <ActivityIndicator color="#944931" size="small" />
              ) : (
                <MaterialIcons color="#944931" name="delete-outline" size={18} />
              )}
            </Pressable>
            <Pressable
              accessibilityLabel={expanded ? 'Collapse entry controls' : 'Expand entry controls'}
              accessibilityRole="button"
              onPress={() => setExpanded((current) => !current)}
              style={({ pressed }) => [styles.entryEditButton, pressed && styles.pressed]}>
              <MaterialIcons
                color="#526049"
                name={expanded ? 'expand-less' : 'expand-more'}
                size={20}
              />
            </Pressable>
          </View>
        </View>

        <View style={styles.entryQuickActions}>
          {updating ? (
            <View style={styles.entryUpdatingPill}>
              <ActivityIndicator color="#526049" size="small" />
              <Text style={styles.entryUpdatingText}>Updating</Text>
            </View>
          ) : null}
          {unopened ? (
            <Pressable
              accessibilityRole="button"
              disabled={busy}
              onPress={() => onOpen(entry)}
              style={({ pressed }) => [
                styles.entryPrimaryQuickButton,
                pressed && styles.pressed,
                busy && styles.disabledButton,
              ]}>
              <MaterialIcons color="#ffffff" name="lock-open" size={16} />
              <Text style={styles.entryPrimaryQuickText}>Open</Text>
            </Pressable>
          ) : null}
        </View>

        {expanded ? (
          <View style={styles.entryExpandedPanel}>
            {opened ? (
              <>
                <View style={styles.entryControlHeader}>
                  <Text style={styles.entryControlLabel}>Remaining</Text>
                  <Text style={styles.entryControlValue}>
                    {formatNumber(entry.currentQuantity)} / {formatNumber(entry.initialQuantity)}{' '}
                    {unitLabel}
                  </Text>
                </View>

                {showFractions ? (
                  <EntryFractionSlider
                    currentQuantity={entry.currentQuantity}
                    disabled={busy}
                    initialQuantity={entry.initialQuantity}
                    onSelect={setFraction}
                  />
                ) : null}

                {showSteppers ? (
                  <View style={styles.entryQuickActions}>
                    <Pressable
                      accessibilityLabel="Decrease remaining quantity"
                      accessibilityRole="button"
                      disabled={busy || !canDecrease}
                      onPress={() => onSetCurrentQuantity(entry, entry.currentQuantity - 1)}
                      style={({ pressed }) => [
                        styles.entryIconQuickButton,
                        pressed && styles.pressed,
                        (busy || !canDecrease) && styles.disabledButton,
                      ]}>
                      <MaterialIcons color="#526049" name="remove" size={18} />
                    </Pressable>
                    <Pressable
                      accessibilityLabel="Increase remaining quantity"
                      accessibilityRole="button"
                      disabled={busy || !canIncrease}
                      onPress={() => onSetCurrentQuantity(entry, entry.currentQuantity + 1)}
                      style={({ pressed }) => [
                        styles.entryIconQuickButton,
                        pressed && styles.pressed,
                        (busy || !canIncrease) && styles.disabledButton,
                      ]}>
                      <MaterialIcons color="#526049" name="add" size={18} />
                    </Pressable>
                    <Pressable
                      accessibilityRole="button"
                      disabled={busy || !canIncrease}
                      onPress={() => onSetCurrentQuantity(entry, entry.initialQuantity)}
                      style={({ pressed }) => [
                        styles.entryQuickButton,
                        pressed && styles.pressed,
                        (busy || !canIncrease) && styles.disabledButton,
                      ]}>
                      <Text style={styles.entryQuickText}>Full</Text>
                    </Pressable>
                    <Pressable
                      accessibilityRole="button"
                      disabled={busy || !canDecrease}
                      onPress={() => onSetCurrentQuantity(entry, 0)}
                      style={({ pressed }) => [
                        styles.entryQuickButton,
                        pressed && styles.pressed,
                        (busy || !canDecrease) && styles.disabledButton,
                      ]}>
                      <Text style={styles.entryQuickText}>Empty</Text>
                    </Pressable>
                  </View>
                ) : null}

                <View style={styles.entryCustomRow}>
                  <TextInput
                    keyboardType="decimal-pad"
                    onChangeText={setCustomQuantity}
                    placeholder="0"
                    placeholderTextColor="#8c8a81"
                    style={styles.entryCustomInput}
                    value={customQuantity}
                  />
                  <Text style={styles.entryCustomUnit}>{unitLabel}</Text>
                  <Pressable
                    accessibilityRole="button"
                    disabled={busy}
                    onPress={applyCustomQuantity}
                    style={({ pressed }) => [
                      styles.entryCustomSaveButton,
                      pressed && styles.pressed,
                      busy && styles.disabledButton,
                    ]}>
                    <Text style={styles.entryPrimaryQuickText}>Save</Text>
                  </Pressable>
                </View>
              </>
            ) : (
              <Text style={styles.entryExpandedText}>
                Open this entry to adjust the remaining amount.
              </Text>
            )}
          </View>
        ) : null}

        <View style={styles.entryMetaGrid}>
          <MetaPill icon="shopping-bag" label={formatDate(entry.acquisitionDate)} />
          <MetaPill icon="event" label={formatDate(entry.expirationDate)} />
          <MetaPill icon="inventory-2" label={formatContainer(entry.containerType)} />
        </View>
      </View>
    </View>
  );
}

function EntryFractionSlider({
  currentQuantity,
  disabled,
  initialQuantity,
  onSelect,
}: {
  currentQuantity: number;
  disabled: boolean;
  initialQuantity: number;
  onSelect: (fraction: number) => void;
}) {
  const selectedValue = getNearestFraction(currentQuantity, initialQuantity);

  return (
    <View style={[styles.entrySlider, disabled && styles.disabledButton]}>
      <View style={styles.entrySliderDotRow}>
        <View style={styles.entrySliderTrack} />
        {fractionOptions.map((option) => {
          const selected = option.value === selectedValue;

          return (
            <Pressable
              accessibilityLabel={`Set remaining to ${option.label}`}
              accessibilityRole="button"
              disabled={disabled}
              key={option.label}
              onPress={() => onSelect(option.value)}
              style={({ pressed }) => [styles.entrySliderStop, pressed && styles.pressed]}>
              <View style={[styles.entrySliderDot, selected && styles.entrySliderDotSelected]} />
            </Pressable>
          );
        })}
      </View>
      <View style={styles.entrySliderLabelRow}>
        {fractionOptions.map((option) => {
          const selected = option.value === selectedValue;

          return (
            <Text
              key={option.label}
              style={[styles.entrySliderLabel, selected && styles.entrySliderLabelSelected]}>
              {option.label}
            </Text>
          );
        })}
      </View>
    </View>
  );
}

function MetaPill({
  icon,
  label,
}: {
  icon: keyof typeof MaterialIcons.glyphMap;
  label: string;
}) {
  return (
    <View style={styles.metaPill}>
      <MaterialIcons color="#757870" name={icon} size={14} />
      <Text numberOfLines={1} style={styles.metaPillText}>
        {label}
      </Text>
    </View>
  );
}

function formatDate(value?: string | null) {
  if (!value) {
    return 'Not set';
  }

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return 'Not set';
  }

  return new Intl.DateTimeFormat(undefined, {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  }).format(date);
}

function formatEntryQuantity(entry: ItemEntryView) {
  if (entry.state === ConsumableState.Unopened) {
    return `${formatNumber(entry.initialQuantity)} ${formatUnit(entry.unit)}`;
  }

  return `${formatNumber(entry.currentQuantity)} / ${formatNumber(entry.initialQuantity)} ${formatUnit(entry.unit)}`;
}

function formatNumber(value: number) {
  return Number.isInteger(value) ? String(value) : value.toLocaleString(undefined, { maximumFractionDigits: 2 });
}

function formatInputQuantity(value: number) {
  return Number.isInteger(value) ? String(value) : String(roundQuantity(value));
}

function parseQuantityInput(value: string) {
  const parsedValue = Number(value.trim().replace(',', '.'));
  return Number.isFinite(parsedValue) ? parsedValue : 0;
}

function roundQuantity(value: number) {
  return Math.round(value * 100) / 100;
}

function getNearestFraction(current: number, initial: number) {
  if (initial <= 0) {
    return 0;
  }

  const fraction = current / initial;
  return fractionOptions.reduce((nearest, option) =>
    Math.abs(option.value - fraction) < Math.abs(nearest.value - fraction) ? option : nearest,
  ).value;
}

function formatUnit(unit: ItemUnit) {
  switch (unit) {
    case ItemUnit.Milliliter:
      return 'ml';
    case ItemUnit.Liter:
      return 'l';
    case ItemUnit.Gram:
      return 'g';
    case ItemUnit.Kilogram:
      return 'kg';
    case ItemUnit.Piece:
      return 'pcs';
    case ItemUnit.Unspecified:
    default:
      return 'pcs';
  }
}

function formatState(state: ConsumableState) {
  switch (state) {
    case ConsumableState.Opened:
      return 'Opened';
    case ConsumableState.Unopened:
      return 'Unopened';
    case ConsumableState.Unspecified:
    default:
      return 'Unknown';
  }
}

function formatContainer(containerType?: ItemContainerType) {
  switch (containerType) {
    case ItemContainerType.Bag:
      return 'Bag';
    case ItemContainerType.Bottle:
      return 'Bottle';
    case ItemContainerType.Box:
      return 'Box';
    case ItemContainerType.Can:
      return 'Can';
    case ItemContainerType.Carton:
      return 'Carton';
    case ItemContainerType.Jar:
      return 'Jar';
    case ItemContainerType.Pack:
      return 'Pack';
    case ItemContainerType.Unspecified:
    default:
      return 'Not set';
  }
}

const styles = StyleSheet.create({
  entryRow: {
    alignItems: 'flex-start',
    backgroundColor: '#faf2ed',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    gap: 12,
    padding: 12,
  },
  entryNumber: {
    alignItems: 'center',
    backgroundColor: '#d8e8cb',
    borderRadius: 8,
    height: 34,
    justifyContent: 'center',
    width: 34,
  },
  entryNumberText: {
    color: '#121f0d',
    fontSize: 14,
    fontWeight: '800',
  },
  entryContent: {
    flex: 1,
    gap: 8,
    minWidth: 0,
  },
  entryTopRow: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 8,
    justifyContent: 'space-between',
  },
  entryTitle: {
    color: '#1e1b18',
    flex: 1,
    fontSize: 15,
    fontWeight: '800',
  },
  entryState: {
    color: '#526049',
    fontSize: 12,
    fontWeight: '800',
  },
  entryActions: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 8,
  },
  entryQuickActions: {
    alignItems: 'center',
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  entryUpdatingPill: {
    alignItems: 'center',
    backgroundColor: '#ffffff',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    gap: 6,
    minHeight: 34,
    paddingHorizontal: 10,
  },
  entryUpdatingText: {
    color: '#526049',
    fontSize: 12,
    fontWeight: '800',
  },
  entryPrimaryQuickButton: {
    alignItems: 'center',
    backgroundColor: '#526049',
    borderRadius: 8,
    flexDirection: 'row',
    gap: 6,
    justifyContent: 'center',
    minHeight: 34,
    paddingHorizontal: 12,
  },
  entryPrimaryQuickText: {
    color: '#ffffff',
    fontSize: 13,
    fontWeight: '800',
  },
  entryIconQuickButton: {
    alignItems: 'center',
    backgroundColor: '#ffffff',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    height: 34,
    justifyContent: 'center',
    width: 34,
  },
  entryQuickButton: {
    alignItems: 'center',
    backgroundColor: '#ffffff',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    justifyContent: 'center',
    minHeight: 34,
    paddingHorizontal: 10,
  },
  entryQuickText: {
    color: '#526049',
    fontSize: 12,
    fontWeight: '800',
  },
  entryExpandedPanel: {
    backgroundColor: '#ffffff',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    gap: 10,
    padding: 10,
  },
  entrySlider: {
    gap: 6,
  },
  entrySliderDotRow: {
    alignItems: 'center',
    flexDirection: 'row',
    minHeight: 22,
  },
  entrySliderTrack: {
    backgroundColor: '#d8e8cb',
    borderRadius: 999,
    height: 4,
    left: 20,
    position: 'absolute',
    right: 20,
  },
  entrySliderLabelRow: {
    flexDirection: 'row',
  },
  entrySliderStop: {
    alignItems: 'center',
    flex: 1,
    minHeight: 34,
    justifyContent: 'center',
  },
  entrySliderDot: {
    backgroundColor: '#ffffff',
    borderColor: '#526049',
    borderRadius: 999,
    borderWidth: 2,
    height: 18,
    width: 18,
  },
  entrySliderDotSelected: {
    backgroundColor: '#526049',
  },
  entrySliderLabel: {
    color: '#757870',
    flex: 1,
    fontSize: 11,
    fontWeight: '800',
    textAlign: 'center',
  },
  entrySliderLabelSelected: {
    color: '#526049',
  },
  entryControlHeader: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 10,
    justifyContent: 'space-between',
  },
  entryControlLabel: {
    color: '#444841',
    fontSize: 12,
    fontWeight: '800',
    textTransform: 'uppercase',
  },
  entryControlValue: {
    color: '#1e1b18',
    fontSize: 14,
    fontWeight: '800',
  },
  entryCustomRow: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 8,
  },
  entryCustomInput: {
    backgroundColor: '#faf2ed',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    color: '#1e1b18',
    flex: 1,
    fontSize: 14,
    fontWeight: '700',
    minHeight: 38,
    minWidth: 80,
    paddingHorizontal: 10,
  },
  entryCustomUnit: {
    color: '#444841',
    fontSize: 13,
    fontWeight: '800',
    minWidth: 28,
  },
  entryCustomSaveButton: {
    alignItems: 'center',
    backgroundColor: '#526049',
    borderRadius: 8,
    justifyContent: 'center',
    minHeight: 38,
    paddingHorizontal: 12,
  },
  entryExpandedText: {
    color: '#444841',
    fontSize: 13,
    lineHeight: 18,
  },
  entryEditButton: {
    alignItems: 'center',
    backgroundColor: '#ffffff',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    height: 34,
    justifyContent: 'center',
    width: 34,
  },
  entryDeleteButton: {
    alignItems: 'center',
    backgroundColor: '#ffdbd0',
    borderColor: '#ffb59e',
    borderRadius: 8,
    borderWidth: 1,
    height: 34,
    justifyContent: 'center',
    width: 34,
  },
  entryMetaGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  metaPill: {
    alignItems: 'center',
    backgroundColor: '#ffffff',
    borderRadius: 8,
    flexDirection: 'row',
    gap: 4,
    minHeight: 28,
    paddingHorizontal: 8,
  },
  metaPillText: {
    color: '#444841',
    fontSize: 12,
    fontWeight: '700',
    maxWidth: 112,
  },
  disabledButton: {
    opacity: 0.5,
  },
  pressed: {
    opacity: 0.78,
  },
});
