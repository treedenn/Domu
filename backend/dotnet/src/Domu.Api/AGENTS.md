# AGENTS.md

This file defines how code should be added and reorganized in `Domu.Api`.

The goal is a feature-based architecture with shared technical layers in the outer folders.

## Intent

- Organize business capabilities under `Features/`.
- Keep cross-cutting technical concerns outside features in `Application/`, `Infrastructure/`, and `Interface/`.
- Prefer small, cohesive feature modules over broad horizontal slices.
- Allow a feature to contain internal groups/categories when that improves clarity.
- Promote a group/category into its own feature when it becomes large enough to stand alone.

## Project Shape

The intended top-level structure is:

```text
Domu.Api/
  Application/
  Infrastructure/
  Interface/
  Features/
    Example/
      Application/
      Domain/
      Infrastructure/
      Interface/
    Users/
    Locations/
```

Meaning:

- `Features/<FeatureName>/` owns a business capability.
- `Features/<FeatureName>/Domain/` contains domain models, value objects, enums, policies, and domain rules.
- `Features/<FeatureName>/Application/` contains use cases, commands, queries, DTOs, orchestration, and ports owned by that feature.
- `Features/<FeatureName>/Infrastructure/` contains implementations used only by that feature.
- `Features/<FeatureName>/Interface/` contains controllers, request/response contracts, and feature-specific transport concerns.
- Outer `Application/`, `Infrastructure/`, and `Interface/` are reserved for truly shared or composition-level concerns.

## Grouping Inside A Feature

A feature may contain groups/categories below its internal layers when the feature has multiple distinct subdomains.

Example:

```text
Features/
  Locations/
    Domain/
      Locations/
      Membership/
      Invitations/
      Items/
    Application/
      Locations/
      Membership/
      Invitations/
      Items/
```

Use this pattern when:

- The sub-area is part of the same broader capability.
- The sub-areas share language, rules, or lifecycle.
- Splitting them into separate features would add more ceremony than value.

## When To Promote A Group Into Its Own Feature

Move a group/category out of an existing feature and make it a top-level feature when most of these become true:

- It has its own terminology and business rules.
- It can evolve independently of the parent feature.
- It needs its own application, infrastructure, and interface flow.
- Other features depend on it as a capability, not just as a detail.
- Navigating the parent feature is getting harder because the group dominates it.

Example:

- `Locations/Items` can stay inside `Locations` while items are conceptually part of the location capability.
- If item behavior, workflows, APIs, persistence, and domain language grow enough, `Items` can become `Features/Items/`.

## Dependency Rules

Keep dependencies pointing inward and keep ownership obvious.

- `Domain` must not depend on `Application`, `Infrastructure`, or `Interface`.
- `Application` may depend on its own feature `Domain`.
- `Infrastructure` may depend on its own feature `Application` and `Domain`.
- `Interface` may depend on its own feature `Application` and on composition root setup.
- A feature should not reach into another feature's infrastructure or interface.
- If one feature needs something from another, depend on a stable application contract or move the shared concept to a better boundary.
- Outer shared folders must not become a dumping ground for code that simply lacks a home.

## What Belongs In Outer Shared Layers

Only place code in root-level shared layers when it is genuinely cross-cutting.

- `Application/`: shared abstractions, mediation primitives, result wrappers, common pipeline behaviors, base contracts.
- `Infrastructure/`: database setup, shared persistence wiring, auth wiring, messaging wiring, shared integrations.
- `Interface/`: shared HTTP concerns, middleware, filters, request context, common response handling.

If a type is only used by one feature, keep it inside that feature.

## Naming And Layout Rules

- Name features by business capability, not technical role.
- Use singular or plural intentionally, then stay consistent.
- Keep namespaces aligned with folders.
- Prefer folders that reflect business language over generic buckets like `Common`, `Helpers`, or `Utils`.
- Avoid `Shared` inside `Features/` unless the scope is explicitly feature-local and cannot be named better.
- Add new folders only when they carry real structural meaning.

## Implementation Guidance

When adding new work:

1. Decide which feature owns the behavior.
2. Decide whether the change belongs in `Domain`, `Application`, `Infrastructure`, or `Interface`.
3. Only introduce a sub-group/category if it clarifies the feature.
4. If the sub-group starts acting like an independent module, extract it into its own feature.

When in doubt:

- Prefer keeping code inside an existing feature.
- Prefer explicit business names over abstract technical names.
- Prefer duplication over premature shared abstractions across features.

## Current Direction

Based on the current codebase:

- `Users` is a feature.
- `Locations` is a feature.
- `Locations` currently contains grouped domain areas such as `Items`, `Membership`, `Invitations`, and `Locations`.
- That grouping is valid and should remain until one of those areas becomes clearly independent enough to extract.

## README Direction

The future `README.md` should stay short and answer:

- What `Domu.Api` is.
- Why the project is organized by feature.
- What belongs in outer shared layers.
- How to add a new feature.
- How to run the API locally.
