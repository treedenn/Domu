# Domu mobile

Flutter client for Domu, organised by business feature to match the backend.

## Structure

```text
lib/
  app/                         # App shell: composition, navigation and theme
    router/
    theme/
  core/                        # Cross-feature primitives only
    api/                        # HTTP client, API errors and interceptors
    auth/                       # Token storage and session primitives
    design_system/              # Reusable visual building blocks
  features/
    households/
      data/                     # DTOs, remote/local sources, repository implementations
      domain/                   # Entities, repository contracts, use cases
      ui/                       # Views and their view models
    auth/                       # OIDC sign-in and authenticated-session behaviour
      data/
      domain/
      ui/
    shopping_lists/
    spaces/
    activities/
    insights/
    users/
```

Create a feature only when it has behaviour to own. Empty placeholder folders
are intentionally not committed.

## Dependency direction

```text
ui -> domain <- data
 ^              |
 +-- app composition
```

- `domain` is plain Dart and does not import Flutter, networking, or storage.
- `data` fulfils domain repository contracts and owns API DTO mapping.
- `ui` contains views and view models. Views render view-model state and invoke
  its commands; view models consume repository contracts, never DTOs or HTTP
  clients directly.
- `app` is the composition root: it wires concrete data implementations into
  feature use cases and owns cross-feature navigation.
- `core` may be used by any layer, but must not depend on a feature.

Keep cross-feature UI in `core/design_system`; feature-specific widgets remain
beside their screen. Prefer constructor injection, which keeps dependencies
easy to test and lets us introduce a state-management or DI package later
without coupling the domain model to it.

## Conventions

- One business capability per `features/<feature>` folder, named after the
  backend feature where practical.
- Public feature entry points belong in `ui`; implementation details
  stay private to their layer.
- Route paths live in `app/router`; a feature exposes its screen or route
  builder, rather than navigating itself across feature boundaries.
- Authentication uses Zitadel through standard OIDC Authorization Code Flow
  with PKCE. Password fields and password handling do not belong in this app.
- Add tests next to the type they exercise, with end-to-end widget flows under
  `integration_test/` when those flows exist.

## Commands

```bash
flutter analyze
flutter test
```

## OIDC configuration

The Android and iOS clients use Zitadel Authorization Code Flow with PKCE and
the `domu://auth/callback` redirect URI. Register that URI for each native
client in Zitadel, enable refresh tokens (`offline_access`), and grant the API
audience/scope accepted by Domu's JWT bearer configuration.

Do not commit environment values. Supply them at launch/build time; when using
local infrastructure, use an issuer and API address reachable from the device
(not `localhost` on the host machine).

For the checked-in local Zitadel stack, keep its configured `localhost` issuer
and use ADB reverse so the Android device's `localhost:8080` reaches the host:

```bash
./tool/adb-reverse.sh
flutter run --dart-define-from-file=env/dev.json
```

The debug Android manifest permits the local stack's HTTP issuer only for debug
builds. Release builds must use an HTTPS issuer with a certificate trusted by
the device.

```bash
flutter run \
  --dart-define=DOMU_OIDC_ISSUER=https://zitadel.example.test \
  --dart-define=DOMU_OIDC_CLIENT_ID=mobile-client-id \
  --dart-define=DOMU_OIDC_REDIRECT_URI=domu://auth/callback \
  --dart-define=DOMU_API_AUDIENCE=domu-api \
  --dart-define=DOMU_API_SCOPE=urn:zitadel:iam:org:project:id:zitadel:aud \
  --dart-define=DOMU_API_BASE_URL=https://api.example.test
```

Use the same defines with `flutter build appbundle` or `flutter build ipa` for
release builds. The app fails at startup with the missing define names when its
configuration is incomplete.

## Zitadel local admin credentials

For the local admin account, the configured credentials are:

- Email: zitadel-admin@zitadel.localhost
- Password: Password1!
