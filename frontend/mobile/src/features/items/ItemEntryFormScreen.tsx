import MaterialIcons from '@expo/vector-icons/MaterialIcons';
import { router, useLocalSearchParams } from 'expo-router';
import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import {
  ActivityIndicator,
  Modal,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { TimeoutError } from '@/core/async/timeout';
import { ApiError } from '@/core/http/apiClient';
import { useAuthSession } from '@/features/auth/authSession';
import {
  ConsumableState,
  getItem,
  getItems,
  ItemContainerType,
  ItemUnit,
  replaceItemEntries,
  type ItemEntryRequest,
  type ItemEntryView,
  type ItemView,
} from '@/features/items/api';
import { AppTopBar } from '@/ui/AppTopBar';

type EntryFormValue = {
  acquisitionDate: string;
  containerType: ItemContainerType;
  currentQuantity: string;
  expirationDate: string;
  initialQuantity: string;
  state: ConsumableState;
  unit: ItemUnit;
};

const itemDetailsRoute = '/households/[householdId]/items/[itemId]' as never;
const unitOptions = [
  { label: 'Pieces', value: ItemUnit.Piece },
  { label: 'ml', value: ItemUnit.Milliliter },
  { label: 'l', value: ItemUnit.Liter },
  { label: 'g', value: ItemUnit.Gram },
  { label: 'kg', value: ItemUnit.Kilogram },
];
const stateOptions = [
  { label: 'Unknown', value: ConsumableState.Unspecified },
  { label: 'Unopened', value: ConsumableState.Unopened },
  { label: 'Opened', value: ConsumableState.Opened },
];
const containerOptions = [
  { label: 'None', value: ItemContainerType.Unspecified },
  { label: 'Bottle', value: ItemContainerType.Bottle },
  { label: 'Carton', value: ItemContainerType.Carton },
  { label: 'Can', value: ItemContainerType.Can },
  { label: 'Jar', value: ItemContainerType.Jar },
  { label: 'Pack', value: ItemContainerType.Pack },
  { label: 'Box', value: ItemContainerType.Box },
  { label: 'Bag', value: ItemContainerType.Bag },
];
const weekdays = ['M', 'T', 'W', 'T', 'F', 'S', 'S'];

export default function ItemEntryFormScreen() {
  const { householdId, itemId, spaceId, entryId } = useLocalSearchParams<{
    householdId?: string | string[];
    itemId?: string | string[];
    spaceId?: string | string[];
    entryId?: string | string[];
  }>();
  const resolvedHouseholdId = firstParam(householdId);
  const resolvedItemId = firstParam(itemId);
  const resolvedSpaceId = firstParam(spaceId);
  const resolvedEntryId = firstParam(entryId);
  const { accessToken, clearTokenResponse } = useAuthSession();
  const [item, setItem] = useState<ItemView | null>(null);
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const returnToSignIn = useCallback(async () => {
    await clearTokenResponse();
    router.replace('/');
  }, [clearTokenResponse]);

  const loadItem = useCallback(async () => {
    if (!accessToken || !resolvedHouseholdId || !resolvedItemId || !resolvedSpaceId) {
      setError('Open an item before editing entries.');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const nextItem = await loadItemDetails(
        resolvedHouseholdId,
        resolvedSpaceId,
        resolvedItemId,
        accessToken,
      );
      setItem(nextItem);
    } catch (exception) {
      if (isExpiredSessionError(exception)) {
        await returnToSignIn();
        return;
      }

      setError(getUserFacingError(exception));
    } finally {
      setLoading(false);
    }
  }, [accessToken, resolvedHouseholdId, resolvedItemId, resolvedSpaceId, returnToSignIn]);

  useEffect(() => {
    loadItem();
  }, [loadItem]);

  const entry = useMemo(
    () => item?.entries.find((candidate) => candidate.id === resolvedEntryId) ?? null,
    [item?.entries, resolvedEntryId],
  );
  const editing = Boolean(resolvedEntryId);

  const saveEntry = useCallback(
    async (value: EntryFormValue) => {
      if (!accessToken || !resolvedHouseholdId || !resolvedItemId || !resolvedSpaceId || !item) {
        setError('Open an item before saving entries.');
        return;
      }

      const parsedCurrentQuantity = Math.max(Number(value.currentQuantity) || 0, 0);
      const initialQuantity = Math.max(Number(value.initialQuantity) || parsedCurrentQuantity || 1, 0);
      const currentQuantity =
        value.state === ConsumableState.Unopened
          ? initialQuantity
          : parsedCurrentQuantity;
      const acquisitionDate = parseDateInput(value.acquisitionDate);

      if (acquisitionDate && isAfterDate(acquisitionDate, new Date())) {
        setError('Bought date cannot be in the future.');
        return;
      }

      const nextEntry: ItemEntryRequest = {
        acquisitionDate: normalizeDateInput(value.acquisitionDate),
        containerType: value.containerType,
        currentQuantity,
        expirationDate: normalizeDateInput(value.expirationDate),
        id: entry?.id ?? null,
        initialQuantity,
        state: value.state,
        unit: value.unit,
      };

      setSaving(true);
      setError(null);

      try {
        await replaceItemEntries(
          resolvedHouseholdId,
          resolvedSpaceId,
          resolvedItemId,
          {
            entries: entry
              ? item.entries.map((candidate) =>
                  candidate.id === entry.id ? nextEntry : toItemEntryRequest(candidate),
                )
              : [...item.entries.map(toItemEntryRequest), nextEntry],
          },
          { accessToken },
        );
        router.replace({
          pathname: itemDetailsRoute,
          params: {
            householdId: resolvedHouseholdId,
            itemId: resolvedItemId,
            spaceId: resolvedSpaceId,
          },
        });
      } catch (exception) {
        if (isExpiredSessionError(exception)) {
          await returnToSignIn();
          return;
        }

        setError(getUserFacingError(exception));
      } finally {
        setSaving(false);
      }
    },
    [
      accessToken,
      entry,
      item,
      resolvedHouseholdId,
      resolvedItemId,
      resolvedSpaceId,
      returnToSignIn,
    ],
  );

  return (
    <SafeAreaView style={styles.safeArea}>
      <ScrollView contentContainerStyle={styles.content} keyboardShouldPersistTaps="handled">
        <AppTopBar
          onBack={() => router.back()}
          subtitle={item?.name ?? 'Item entry'}
          title={editing ? 'Edit Entry' : 'Add New Entry'}
        />

        {loading ? (
          <View style={styles.loadingPanel}>
            <ActivityIndicator color="#526049" />
            <Text style={styles.loadingText}>Loading entry</Text>
          </View>
        ) : null}

        {error ? (
          <View style={styles.errorPanel}>
            <MaterialIcons color="#944931" name="error-outline" size={22} />
            <Text style={styles.errorText}>{error}</Text>
          </View>
        ) : null}

        {item && (!editing || entry) ? (
          <EntryForm entry={entry} onSubmit={saveEntry} saving={saving} />
        ) : null}
      </ScrollView>
    </SafeAreaView>
  );
}

function EntryForm({
  entry,
  onSubmit,
  saving,
}: {
  entry: ItemEntryView | null;
  onSubmit: (value: EntryFormValue) => void;
  saving: boolean;
}) {
  const [currentQuantity, setCurrentQuantity] = useState(String(entry?.currentQuantity ?? 1));
  const [initialQuantity, setInitialQuantity] = useState(String(entry?.initialQuantity ?? 1));
  const [unit, setUnit] = useState(entry?.unit ?? ItemUnit.Piece);
  const [state, setState] = useState(entry?.state ?? ConsumableState.Unopened);
  const [containerType, setContainerType] = useState(entry?.containerType ?? ItemContainerType.Unspecified);
  const [acquisitionDate, setAcquisitionDate] = useState(formatDateInput(entry?.acquisitionDate));
  const [expirationDate, setExpirationDate] = useState(formatDateInput(entry?.expirationDate));
  const canSave = !saving;
  const showRemainingQuantity = state !== ConsumableState.Unopened;

  useEffect(() => {
    setCurrentQuantity(String(entry?.currentQuantity ?? 1));
    setInitialQuantity(String(entry?.initialQuantity ?? 1));
    setUnit(entry?.unit ?? ItemUnit.Piece);
    setState(entry?.state ?? ConsumableState.Unopened);
    setContainerType(entry?.containerType ?? ItemContainerType.Unspecified);
    setAcquisitionDate(formatDateInput(entry?.acquisitionDate));
    setExpirationDate(formatDateInput(entry?.expirationDate));
  }, [entry]);

  const submit = useCallback(() => {
    onSubmit({
      acquisitionDate,
      containerType,
      currentQuantity: showRemainingQuantity ? currentQuantity : initialQuantity,
      expirationDate,
      initialQuantity,
      state,
      unit,
    });
  }, [
    acquisitionDate,
    containerType,
    currentQuantity,
    expirationDate,
    initialQuantity,
    onSubmit,
    showRemainingQuantity,
    state,
    unit,
  ]);

  return (
    <View style={styles.formPanel}>
      <View style={styles.formRow}>
        {showRemainingQuantity ? (
          <FormField label="Remaining">
            <TextInput
              keyboardType="decimal-pad"
              onChangeText={setCurrentQuantity}
              placeholder="0"
              placeholderTextColor="#8c8a81"
              style={styles.input}
              value={currentQuantity}
            />
          </FormField>
        ) : null}
        <FormField label="Initial">
          <TextInput
            keyboardType="decimal-pad"
            onChangeText={setInitialQuantity}
            placeholder="1"
            placeholderTextColor="#8c8a81"
            style={styles.input}
            value={initialQuantity}
          />
        </FormField>
      </View>

      <ChoiceGroup label="Unit" onSelect={setUnit} options={unitOptions} selected={unit} />
      <ChoiceGroup label="State" onSelect={setState} options={stateOptions} selected={state} />
      <ChoiceGroup
        label="Container"
        onSelect={setContainerType}
        options={containerOptions}
        selected={containerType}
      />

      <View style={styles.formRow}>
        <FormField label="Bought">
          <DateInput maxDate={new Date()} onChange={setAcquisitionDate} value={acquisitionDate} />
        </FormField>
        <FormField label="Expires">
          <DateInput onChange={setExpirationDate} value={expirationDate} />
        </FormField>
      </View>

      <Pressable
        accessibilityRole="button"
        disabled={!canSave}
        onPress={submit}
        style={({ pressed }) => [
          styles.saveButton,
          pressed && styles.pressed,
          !canSave && styles.disabledButton,
        ]}>
        {saving ? (
          <ActivityIndicator color="#ffffff" />
        ) : (
          <>
            <MaterialIcons color="#ffffff" name="save" size={20} />
            <Text style={styles.saveButtonText}>{entry ? 'Save Entry' : 'Add Entry'}</Text>
          </>
        )}
      </Pressable>
    </View>
  );
}

function ChoiceGroup<T extends string | number>({
  label,
  onSelect,
  options,
  selected,
}: {
  label: string;
  onSelect: (value: T) => void;
  options: { label: string; value: T }[];
  selected: T;
}) {
  return (
    <View style={styles.field}>
      <Text style={styles.label}>{label}</Text>
      <View style={styles.choiceGrid}>
        {options.map((option) => (
          <Pressable
            accessibilityRole="button"
            key={String(option.value)}
            onPress={() => onSelect(option.value)}
            style={({ pressed }) => [
              styles.choiceChip,
              selected === option.value && styles.choiceChipActive,
              pressed && styles.pressed,
            ]}>
            <Text
              style={[
                styles.choiceChipText,
                selected === option.value && styles.choiceChipTextActive,
              ]}>
              {option.label}
            </Text>
          </Pressable>
        ))}
      </View>
    </View>
  );
}

function DateInput({
  maxDate,
  onChange,
  value,
}: {
  maxDate?: Date;
  onChange: (value: string) => void;
  value: string;
}) {
  const [calendarVisible, setCalendarVisible] = useState(false);
  const selectedDate = parseDateInput(value);
  const [visibleMonth, setVisibleMonth] = useState(() => selectedDate ?? startOfMonth(new Date()));
  const displayValue = selectedDate ? formatDate(selectedDate.toISOString()) : 'Not set';
  const normalizedMaxDate = maxDate ? startOfDay(maxDate) : null;

  useEffect(() => {
    if (calendarVisible) {
      setVisibleMonth(selectedDate ?? startOfMonth(new Date()));
    }
  }, [calendarVisible, selectedDate]);

  const selectDate = useCallback(
    (date: Date) => {
      onChange(formatDateInputValue(date));
      setCalendarVisible(false);
    },
    [onChange],
  );

  return (
    <>
      <Pressable
        accessibilityRole="button"
        onPress={() => setCalendarVisible(true)}
        style={({ pressed }) => [styles.dateSelectButton, pressed && styles.pressed]}>
        <MaterialIcons color="#526049" name="calendar-month" size={18} />
        <Text
          numberOfLines={1}
          style={[styles.dateValueText, !selectedDate && styles.dateValueTextEmpty]}>
          {displayValue}
        </Text>
        <MaterialIcons color="#757870" name="expand-more" size={18} />
      </Pressable>
      <Modal
        animationType="fade"
        transparent
        visible={calendarVisible}
        onRequestClose={() => setCalendarVisible(false)}>
        <Pressable style={styles.calendarBackdrop} onPress={() => setCalendarVisible(false)}>
          <Pressable style={styles.calendarPanel}>
            <View style={styles.calendarHeader}>
              <Pressable
                accessibilityLabel="Previous month"
                accessibilityRole="button"
                onPress={() => setVisibleMonth((date) => addMonths(date, -1))}
                style={({ pressed }) => [styles.calendarNavButton, pressed && styles.pressed]}>
                <MaterialIcons color="#526049" name="chevron-left" size={22} />
              </Pressable>
              <Text style={styles.calendarTitle}>{formatMonthTitle(visibleMonth)}</Text>
              <Pressable
                accessibilityLabel="Next month"
                accessibilityRole="button"
                onPress={() => setVisibleMonth((date) => addMonths(date, 1))}
                style={({ pressed }) => [styles.calendarNavButton, pressed && styles.pressed]}>
                <MaterialIcons color="#526049" name="chevron-right" size={22} />
              </Pressable>
            </View>

            <View style={styles.weekdayRow}>
              {weekdays.map((weekday, index) => (
                <Text key={`${weekday}-${index}`} style={styles.weekdayText}>
                  {weekday}
                </Text>
              ))}
            </View>

            <View style={styles.calendarGrid}>
              {buildCalendarDays(visibleMonth).map((date, index) => {
                const inMonth = date.getMonth() === visibleMonth.getMonth();
                const selected = selectedDate ? isSameDate(date, selectedDate) : false;
                const disabled = normalizedMaxDate ? isAfterDate(date, normalizedMaxDate) : false;

                return (
                  <Pressable
                    accessibilityRole="button"
                    disabled={disabled}
                    key={`${index}-${date.getFullYear()}-${date.getMonth()}-${date.getDate()}`}
                    onPress={() => selectDate(date)}
                    style={({ pressed }) => [
                      styles.calendarDay,
                      !inMonth && styles.calendarDayMuted,
                      disabled && styles.calendarDayDisabled,
                      selected && styles.calendarDaySelected,
                      pressed && styles.pressed,
                    ]}>
                    <Text
                      style={[
                        styles.calendarDayText,
                        !inMonth && styles.calendarDayTextMuted,
                        disabled && styles.calendarDayTextDisabled,
                        selected && styles.calendarDayTextSelected,
                      ]}>
                      {date.getDate()}
                    </Text>
                  </Pressable>
                );
              })}
            </View>

            <View style={styles.calendarActions}>
              <Pressable
                accessibilityRole="button"
                onPress={() => {
                  onChange('');
                  setCalendarVisible(false);
                }}
                style={({ pressed }) => [styles.calendarClearButton, pressed && styles.pressed]}>
                <Text style={styles.calendarClearText}>Clear</Text>
              </Pressable>
              <Pressable
                accessibilityRole="button"
                onPress={() => setCalendarVisible(false)}
                style={({ pressed }) => [styles.calendarDoneButton, pressed && styles.pressed]}>
                <Text style={styles.calendarDoneText}>Done</Text>
              </Pressable>
            </View>
          </Pressable>
        </Pressable>
      </Modal>
    </>
  );
}

function FormField({ children, label }: { children: ReactNode; label: string }) {
  return (
    <View style={styles.field}>
      <Text style={styles.label}>{label}</Text>
      {children}
    </View>
  );
}

async function loadItemDetails(
  householdId: string,
  spaceId: string,
  itemId: string,
  accessToken: string,
) {
  try {
    return await getItem(householdId, spaceId, itemId, { accessToken });
  } catch (exception) {
    if (exception instanceof ApiError && (exception.status === 404 || exception.status === 405)) {
      const items = await getItems(householdId, spaceId, { accessToken });
      const item = items.find((candidate) => candidate.id === itemId);

      if (item) {
        return item;
      }
    }

    throw exception;
  }
}

function toItemEntryRequest(entry: ItemEntryView): ItemEntryRequest {
  return {
    acquisitionDate: entry.acquisitionDate,
    containerType: entry.containerType,
    currentQuantity: entry.currentQuantity,
    expirationDate: entry.expirationDate,
    id: entry.id,
    initialQuantity: entry.initialQuantity,
    state: entry.state,
    unit: entry.unit,
  };
}

function firstParam(value?: string | string[]) {
  return Array.isArray(value) ? value[0] : value;
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

function formatDateInput(value?: string | null) {
  if (!value) {
    return '';
  }

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return '';
  }

  return date.toISOString().slice(0, 10);
}

function parseDateInput(value: string) {
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value.trim());

  if (!match) {
    return null;
  }

  const year = Number(match[1]);
  const month = Number(match[2]) - 1;
  const day = Number(match[3]);
  const date = new Date(year, month, day);

  if (date.getFullYear() !== year || date.getMonth() !== month || date.getDate() !== day) {
    return null;
  }

  return date;
}

function formatDateInputValue(date: Date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function startOfMonth(date: Date) {
  return new Date(date.getFullYear(), date.getMonth(), 1);
}

function addMonths(date: Date, months: number) {
  return new Date(date.getFullYear(), date.getMonth() + months, 1);
}

function formatMonthTitle(date: Date) {
  return new Intl.DateTimeFormat(undefined, {
    month: 'long',
    year: 'numeric',
  }).format(date);
}

function buildCalendarDays(month: Date) {
  const firstDay = startOfMonth(month);
  const mondayOffset = (firstDay.getDay() + 6) % 7;
  const firstVisibleDate = new Date(
    firstDay.getFullYear(),
    firstDay.getMonth(),
    firstDay.getDate() - mondayOffset,
  );

  return Array.from({ length: 42 }, (_, index) => (
    new Date(
      firstVisibleDate.getFullYear(),
      firstVisibleDate.getMonth(),
      firstVisibleDate.getDate() + index,
    )
  ));
}

function isSameDate(left: Date, right: Date) {
  return (
    left.getFullYear() === right.getFullYear() &&
    left.getMonth() === right.getMonth() &&
    left.getDate() === right.getDate()
  );
}

function isAfterDate(left: Date, right: Date) {
  return startOfDay(left).getTime() > startOfDay(right).getTime();
}

function startOfDay(date: Date) {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate());
}

function normalizeDateInput(value: string) {
  const trimmedValue = value.trim();

  if (!trimmedValue) {
    return null;
  }

  const date = new Date(trimmedValue);

  if (Number.isNaN(date.getTime())) {
    return trimmedValue;
  }

  return date.toISOString();
}

function getUserFacingError(exception: unknown) {
  if (exception instanceof TimeoutError) {
    return `${exception.message} Check adb reverse or EXPO_PUBLIC_API_URL.`;
  }

  if (exception instanceof ApiError) {
    if (exception.status === 401) {
      return 'Your session is missing or expired. Sign in again.';
    }

    if (exception.status === 404) {
      return 'This item was not found.';
    }

    return exception.message;
  }

  return 'Check that the backend is running at the configured API URL.';
}

function isExpiredSessionError(exception: unknown) {
  return exception instanceof ApiError && exception.status === 401;
}

const styles = StyleSheet.create({
  safeArea: {
    backgroundColor: '#fff8f3',
    flex: 1,
  },
  content: {
    gap: 20,
    padding: 20,
    paddingBottom: 36,
  },
  loadingPanel: {
    alignItems: 'center',
    flexDirection: 'row',
    gap: 10,
  },
  loadingText: {
    color: '#444841',
    fontSize: 14,
    fontWeight: '700',
  },
  errorPanel: {
    alignItems: 'flex-start',
    backgroundColor: '#ffdbd0',
    borderColor: '#ffb59e',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    gap: 12,
    padding: 14,
  },
  errorText: {
    color: '#76321c',
    flex: 1,
    fontSize: 14,
    lineHeight: 20,
  },
  formPanel: {
    backgroundColor: '#ffffff',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    gap: 18,
    padding: 16,
    shadowColor: '#5c5854',
    shadowOffset: { height: 4, width: 0 },
    shadowOpacity: 0.05,
    shadowRadius: 12,
  },
  formRow: {
    flexDirection: 'row',
    gap: 10,
  },
  field: {
    flex: 1,
    gap: 8,
  },
  label: {
    color: '#444841',
    fontSize: 13,
    fontWeight: '800',
  },
  input: {
    backgroundColor: '#faf2ed',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    color: '#1e1b18',
    fontSize: 15,
    minHeight: 46,
    paddingHorizontal: 12,
  },
  choiceGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 8,
  },
  choiceChip: {
    alignItems: 'center',
    backgroundColor: '#ffffff',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    justifyContent: 'center',
    minHeight: 36,
    paddingHorizontal: 10,
  },
  choiceChipActive: {
    backgroundColor: '#d8e8cb',
    borderColor: '#526049',
  },
  choiceChipText: {
    color: '#444841',
    fontSize: 12,
    fontWeight: '800',
  },
  choiceChipTextActive: {
    color: '#121f0d',
  },
  dateSelectButton: {
    alignItems: 'center',
    backgroundColor: '#faf2ed',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    flexDirection: 'row',
    gap: 8,
    justifyContent: 'space-between',
    minHeight: 46,
    paddingHorizontal: 12,
  },
  dateValueText: {
    color: '#1e1b18',
    flex: 1,
    fontSize: 15,
    fontWeight: '700',
  },
  dateValueTextEmpty: {
    color: '#8c8a81',
  },
  calendarBackdrop: {
    alignItems: 'center',
    backgroundColor: 'rgba(30, 27, 24, 0.24)',
    flex: 1,
    justifyContent: 'center',
    padding: 20,
  },
  calendarPanel: {
    backgroundColor: '#ffffff',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    gap: 14,
    padding: 16,
    width: '100%',
  },
  calendarHeader: {
    alignItems: 'center',
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  calendarNavButton: {
    alignItems: 'center',
    backgroundColor: '#faf2ed',
    borderColor: '#e8e1dc',
    borderRadius: 8,
    borderWidth: 1,
    height: 40,
    justifyContent: 'center',
    width: 40,
  },
  calendarTitle: {
    color: '#1e1b18',
    fontSize: 17,
    fontWeight: '800',
  },
  weekdayRow: {
    flexDirection: 'row',
  },
  weekdayText: {
    color: '#757870',
    flex: 1,
    fontSize: 12,
    fontWeight: '800',
    textAlign: 'center',
  },
  calendarGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
  },
  calendarDay: {
    alignItems: 'center',
    borderRadius: 8,
    height: 38,
    justifyContent: 'center',
    width: `${100 / 7}%`,
  },
  calendarDayMuted: {
    opacity: 0.42,
  },
  calendarDayDisabled: {
    opacity: 0.24,
  },
  calendarDaySelected: {
    backgroundColor: '#526049',
  },
  calendarDayText: {
    color: '#1e1b18',
    fontSize: 14,
    fontWeight: '800',
  },
  calendarDayTextMuted: {
    color: '#757870',
  },
  calendarDayTextDisabled: {
    color: '#757870',
  },
  calendarDayTextSelected: {
    color: '#ffffff',
  },
  calendarActions: {
    flexDirection: 'row',
    gap: 10,
  },
  calendarClearButton: {
    alignItems: 'center',
    backgroundColor: '#ffffff',
    borderColor: '#c5c8be',
    borderRadius: 8,
    borderWidth: 1,
    flex: 1,
    justifyContent: 'center',
    minHeight: 42,
  },
  calendarClearText: {
    color: '#444841',
    fontSize: 14,
    fontWeight: '800',
  },
  calendarDoneButton: {
    alignItems: 'center',
    backgroundColor: '#526049',
    borderRadius: 8,
    flex: 1,
    justifyContent: 'center',
    minHeight: 42,
  },
  calendarDoneText: {
    color: '#ffffff',
    fontSize: 14,
    fontWeight: '800',
  },
  saveButton: {
    alignItems: 'center',
    backgroundColor: '#526049',
    borderRadius: 8,
    flexDirection: 'row',
    gap: 8,
    justifyContent: 'center',
    minHeight: 52,
  },
  saveButtonText: {
    color: '#ffffff',
    fontSize: 16,
    fontWeight: '800',
  },
  disabledButton: {
    opacity: 0.5,
  },
  pressed: {
    opacity: 0.78,
  },
});
