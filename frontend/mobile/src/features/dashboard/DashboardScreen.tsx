import { StyleSheet, Text, View } from 'react-native';

export default function DashboardScreen() {
  return (
    <View style={styles.screen}>
      <View style={styles.header}>
        <Text style={styles.eyebrow}>Domu</Text>
        <Text style={styles.title}>Dashboard</Text>
        <Text style={styles.body}>You are signed in. This is the first authenticated screen.</Text>
      </View>

      <View style={styles.panel}>
        <Text style={styles.panelTitle}>Today</Text>
        <Text style={styles.panelText}>Your home overview will live here.</Text>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  screen: {
    flex: 1,
    backgroundColor: '#f6f4ef',
    padding: 24,
    paddingTop: 72,
  },
  header: {
    gap: 10,
  },
  eyebrow: {
    color: '#4f6f52',
    fontSize: 15,
    fontWeight: '700',
    letterSpacing: 0,
    textTransform: 'uppercase',
  },
  title: {
    color: '#19201a',
    fontSize: 34,
    fontWeight: '800',
    letterSpacing: 0,
    lineHeight: 40,
  },
  body: {
    color: '#4b544c',
    fontSize: 17,
    lineHeight: 25,
    maxWidth: 380,
  },
  panel: {
    backgroundColor: '#ffffff',
    borderColor: '#ded9cb',
    borderRadius: 8,
    borderWidth: 1,
    gap: 8,
    marginTop: 32,
    padding: 16,
  },
  panelTitle: {
    color: '#19201a',
    fontSize: 18,
    fontWeight: '700',
  },
  panelText: {
    color: '#4b544c',
    fontSize: 15,
    lineHeight: 22,
  },
});
