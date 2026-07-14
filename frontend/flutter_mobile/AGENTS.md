# Flutter mobile architecture

Follow Flutter's recommended MVVM separation for every feature:

- Put widgets and feature-specific view models in `features/<feature>/ui/`.
- Keep views focused on rendering state and forwarding user events to their
  view model. Do not put data access or business logic in widgets.
- Put UI state, presentation logic, and user-action commands in view models.
  View models depend on repository contracts, never HTTP clients, storage, DTOs,
  or other data-source implementations.
- Put repository implementations, services, storage, and external API clients
  in `features/<feature>/data/`. Repositories implement contracts in `domain/`.
- Keep `domain/` free of Flutter and data-source implementation imports.
- Wire concrete dependencies only in the app composition root (`lib/main.dart`
  and `lib/app/`).

Use Conventional Commit messages: `<type>(<scope>): <concise imperative
description>`. Use lowercase type and scope, omit the scope only when no
meaningful scope exists, and keep each commit to one logical change.
