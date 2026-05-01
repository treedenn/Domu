# domu_mobile_app

Domu Mobile App

## Configuration

The app reads configuration from Dart compile-time environment values in
`lib/app/bootstrap/app_config.dart`.

For local development, copy the example config and edit the values:

```powershell
Copy-Item config\app_config.example.json config\app_config.local.json
```

Run the app with that config file:

```powershell
fvm flutter run --dart-define-from-file=config\app_config.local.json
```

When running against simple HTTP services on the host machine from the Android
emulator, `10.0.2.2` can be used instead of `localhost`, for example
`http://10.0.2.2:5070`.

For Zitadel/OIDC, keep the issuer host aligned with the Zitadel external domain.
The local infrastructure defaults to `ZITADEL_DOMAIN=localhost`, so the Android
emulator should use `OIDC_ISSUER=http://localhost:8080` together with an ADB
reverse port:

```powershell
adb reverse tcp:8080 tcp:8080
```

If the API is also reached through `localhost` from the emulator, reverse that
port too:

```powershell
adb reverse tcp:5070 tcp:5070
```

Debug and profile Android builds allow cleartext HTTP for local development;
release builds should use HTTPS.

The local config file is ignored by git. Keep the checked-in
`config/app_config.example.json` updated when new config keys are added.

## Getting Started

This project is a starting point for a Flutter application.

A few resources to get you started if this is your first Flutter project:

- [Lab: Write your first Flutter app](https://docs.flutter.dev/get-started/codelab)
- [Cookbook: Useful Flutter samples](https://docs.flutter.dev/cookbook)

For help getting started with Flutter development, view the
[online documentation](https://docs.flutter.dev/), which offers tutorials,
samples, guidance on mobile development, and a full API reference.
