# AGENTS.md

This file defines how code should be added, reorganized, and verified in the
`backend/dotnet` solution.

The solution contains two projects:

- `src/Domu.Api/Domu.Api.csproj`: ASP.NET API and production code.
- `tests/Domu.Tests/Domu.Tests.csproj`: xUnit tests for `Domu.Api`.

## Solution Intent

- Keep the API organized by business feature.
- Keep tests aligned with the production feature and layer they verify.
- Keep cross-cutting technical concerns outside features in the outer shared
  folders.
- Prefer small, cohesive feature modules over broad horizontal slices.
- Allow a feature to contain internal groups/categories when that improves
  clarity.
- Promote a group/category into its own feature when it becomes large enough to
  stand alone.

## Solution Shape

```text
backend/dotnet/
  Domu.sln
  src/
    Domu.Api/
  tests/
    Domu.Tests/
```

Use paths in this file relative to the repository root unless a command says
otherwise.

## Domu.Api

`Domu.Api` owns all production behavior for the backend API.

### API Project Shape

The intended top-level structure is:

```text
src/Domu.Api/
  Application/
  Infrastructure/
  Interface/
  Features/
    Example/
      Application/
      Domain/
      Infrastructure/
      Interface/
    Households/
    Users/
    Spaces/
```

Meaning:

- `Features/<FeatureName>/` owns a business capability.
- `Features/<FeatureName>/Domain/` contains domain models, value objects, enums,
  policies, and domain rules.
- `Features/<FeatureName>/Application/` contains use cases, commands, queries,
  contracts, orchestration, and ports owned by that feature.
- `Features/<FeatureName>/Infrastructure/` contains implementations used only by
  that feature.
- `Features/<FeatureName>/Interface/` contains controllers, request/response
  contracts, and feature-specific transport concerns.
- Outer `Application/`, `Infrastructure/`, and `Interface/` are reserved for
  truly shared or composition-level concerns.

### Grouping Inside An API Feature

A feature may contain groups/categories below its internal layers when the
feature has multiple distinct subdomains.

Example:

```text
Features/
  Spaces/
    Domain/
      Spaces/
      Items/
    Application/
      Spaces/
      Items/
```

Use this pattern when:

- The sub-area is part of the same broader capability.
- The sub-areas share language, rules, or lifecycle.
- Splitting them into separate features would add more ceremony than value.

### When To Promote A Group Into Its Own Feature

Move a group/category out of an existing feature and make it a top-level feature
when most of these become true:

- It has its own terminology and business rules.
- It can evolve independently of the parent feature.
- It needs its own application, infrastructure, and interface flow.
- Other features depend on it as a capability, not just as a detail.
- Navigating the parent feature is getting harder because the group dominates it.

Example:

- `Spaces/Items` can stay inside `Spaces` while items are conceptually part of
  the Space capability.
- If item behavior, workflows, APIs, persistence, and domain language grow enough,
  `Items` can become `Features/Items/`.

### API Dependency Rules

Keep dependencies pointing inward and keep ownership obvious.

- `Domain` must not depend on `Application`, `Infrastructure`, or `Interface`.
- `Application` may depend on its own feature `Domain`.
- `Infrastructure` may depend on its own feature `Application` and `Domain`.
- `Interface` may depend on its own feature `Application` and on composition root
  setup.
- A feature should not reach into another feature's infrastructure or interface.
- If one feature needs something from another, depend on a stable application
  contract or move the shared concept to a better boundary.
- Outer shared folders must not become a dumping ground for code that simply lacks
  a home.

### What Belongs In API Outer Shared Layers

Only place code in root-level shared layers when it is genuinely cross-cutting.

- `Application/`: shared abstractions, mediation primitives, result wrappers,
  common pipeline behaviors, base contracts.
- `Infrastructure/`: database setup, shared persistence wiring, auth wiring,
  messaging wiring, shared integrations.
- `Interface/`: shared HTTP concerns, middleware, filters, request context, common
  response handling.

If a type is only used by one feature, keep it inside that feature.

### API Naming And Layout Rules

- Name features by business capability, not technical role.
- Use singular or plural intentionally, then stay consistent.
- Keep namespaces aligned with folders.
- Prefer folders that reflect business language over generic buckets like
  `Common`, `Helpers`, or `Utils`.
- Avoid `Shared` inside `Features/` unless the scope is explicitly feature-local
  and cannot be named better.
- Add new folders only when they carry real structural meaning.

### API Implementation Guidance

When adding production behavior:

1. Decide which feature owns the behavior.
2. Decide whether the change belongs in `Domain`, `Application`,
   `Infrastructure`, or `Interface`.
3. Only introduce a sub-group/category if it clarifies the feature.
4. If the sub-group starts acting like an independent module, extract it into its
   own feature.

Do not create, remove, or regenerate Entity Framework migrations. Agents may
update EF models and configurations, but migration files must be created by the
project owner unless the user explicitly instructs otherwise in the current
turn.

When in doubt:

- Prefer keeping code inside an existing feature.
- Prefer explicit business names over abstract technical names.
- Prefer duplication over premature shared abstractions across features.

### Current API Direction

Based on the current codebase:

- `Users` is a feature.
- `Households` is a feature.
- `Spaces` is a feature.
- `Spaces` currently contains grouped domain areas such as `Items` and `Spaces`.
- That grouping is valid and should remain until one of those areas becomes
  clearly independent enough to extract.

## Domu.Tests

`Domu.Tests` verifies `Domu.Api`. It should mirror the API's feature structure so
tests stay easy to locate from the production code they cover.

### Test Project Shape

The intended top-level structure is:

```text
tests/Domu.Tests/
  Features/
    Households/
      Application/
      Domain/
      Infrastructure/
      Interface/
    Spaces/
      Application/
      Domain/
      Infrastructure/
      Interface/
    Users/
      Application/
      Domain/
      Infrastructure/
      Interface/
```

Only create layer folders that contain real tests. Empty future folders are not
needed.

### Test Placement Rules

- Put tests under the same feature name as the production code.
- Put tests under the same layer they primarily verify: `Domain`, `Application`,
  `Infrastructure`, or `Interface`.
- Name test files after the production type or behavior they verify, using the
  `Tests` suffix.
- Keep domain tests focused on domain behavior and invariants.
- Keep application tests focused on use case behavior, commands, queries, and
  repository port interactions.
- Keep infrastructure tests focused on persistence mapping, entity behavior, and
  integration details.
- Add interface tests when HTTP behavior, routing, request contracts, or response
  contracts need coverage.

### Test Dependency Rules

- `Domu.Tests` may reference `Domu.Api`.
- Tests should not introduce production-only abstractions into `Domu.Api` just to
  make testing easier.
- Prefer simple test doubles in the relevant test file unless the same setup is
  repeated enough to justify a small helper.
- Keep test helpers scoped to the feature or layer that uses them before creating
  broader shared test utilities.

### Test Implementation Guidance

When adding or changing production behavior:

1. Add or update tests in the matching `tests/Domu.Tests/Features/<Feature>/`
   folder when the behavior has meaningful logic or risk.
2. Match the test layer to the production layer being verified.
3. Prefer behavior-focused test names over implementation-detail names.
4. Keep tests independent and deterministic.

## README Direction

The future `README.md` for `backend/dotnet` should stay short and answer:

- What the dotnet solution contains.
- What `Domu.Api` is.
- What `Domu.Tests` verifies.
- Why the API project is organized by feature.
- What belongs in API outer shared layers.
- How to add a new feature and its tests.
- How to run the API and tests locally.

## Build And Test Commands

Use paths relative to the `domu` project root:

- Solution: `backend/dotnet/Domu.sln`
- API project: `backend/dotnet/src/Domu.Api/Domu.Api.csproj`
- Test project: `backend/dotnet/tests/Domu.Tests/Domu.Tests.csproj`

Do not run `dotnet ef migrations add`, `dotnet ef migrations remove`, or other
commands that create, remove, or regenerate migration files unless the user
explicitly asks for that action in the current turn.

Before running `dotnet build` or `dotnet test`, set these environment variables
in the same PowerShell command:

```powershell
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'
$env:DOTNET_CLI_TELEMETRY_OPTOUT='1'
$env:DOTNET_NOLOGO='1'
$env:DOTNET_CLI_HOME='backend/dotnet/src/Domu.Api/.dotnet'
```

Preferred build command:

```powershell
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'; $env:DOTNET_CLI_TELEMETRY_OPTOUT='1'; $env:DOTNET_NOLOGO='1'; $env:DOTNET_CLI_HOME='backend/dotnet/src/Domu.Api/.dotnet'; dotnet build 'backend/dotnet/Domu.sln' -v minimal
```

Preferred test command:

```powershell
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'; $env:DOTNET_CLI_TELEMETRY_OPTOUT='1'; $env:DOTNET_NOLOGO='1'; $env:DOTNET_CLI_HOME='backend/dotnet/src/Domu.Api/.dotnet'; dotnet test 'backend/dotnet/tests/Domu.Tests/Domu.Tests.csproj' --no-restore -v minimal
```
