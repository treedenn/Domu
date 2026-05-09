import { DarkTheme, DefaultTheme, ThemeProvider } from '@react-navigation/native';
import { Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import 'react-native-reanimated';

import { AuthSessionProvider } from '@/features/auth/authSession';
import { useColorScheme } from '@/hooks/use-color-scheme';

export default function RootLayout() {
  const colorScheme = useColorScheme();

  return (
    <AuthSessionProvider>
      <ThemeProvider value={colorScheme === 'dark' ? DarkTheme : DefaultTheme}>
        <Stack screenOptions={{ headerShown: false }}>
          <Stack.Screen name="index" />
          <Stack.Screen name="auth/callback" />
          <Stack.Screen name="dashboard" />
          <Stack.Screen name="households" />
          <Stack.Screen name="households/[householdId]/index" />
          <Stack.Screen name="households/[householdId]/spaces" />
          <Stack.Screen name="households/[householdId]/items/add" />
          <Stack.Screen name="households/[householdId]/items/scanner" />
          <Stack.Screen name="households/[householdId]/items/basket" />
          <Stack.Screen name="households/[householdId]/items/[itemId]/entry" />
          <Stack.Screen name="households/[householdId]/items/[itemId]" />
        </Stack>
        <StatusBar style="auto" />
      </ThemeProvider>
    </AuthSessionProvider>
  );
}
