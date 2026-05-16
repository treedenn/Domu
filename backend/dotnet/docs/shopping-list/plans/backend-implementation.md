# Domu Shopping List Plan

This plan adds a household-scoped shopping list to the Domu .NET backend. It is
organized for small implementation passes and should be used from this
`backend/dotnet`-relative file:

```txt
docs/shopping-list/plans/backend-implementation.md
```

When asking Codex to implement work from this plan:

```txt
Read backend/dotnet/docs/shopping-list/plans/backend-implementation.md.
Implement only the requested phase.
Follow existing architecture, naming, validation, authorization, and test conventions.
Do not implement later phases unless explicitly asked.
Do not create, remove, or regenerate EF migrations unless explicitly asked in the current turn.
```

## V1 Outcome

Household members can open one default shopping list for a household, add and
manage items, and see updates from other household members near-instantly.

V1 includes:

- One active default shopping list per household.
- Quick-add items by name.
- Edit item name, quantity, unit, note, optional Space link, and optional Item link.
- Check, uncheck, delete, and clear checked items.
- Add to shopping list from a Space.
- Add an existing household Item to the shopping list.
- Realtime refresh after item mutations.

V1 excludes:

- Stores, prices, categories, barcode scanning, AI parsing, meal planning,
  recurring purchases, purchase history, advanced duplicate merging, and unit
  conversion.

Key data rule:

```txt
Each shopping list item is its own database record.
Do not store items as a JSON array on ShoppingList.
```

This keeps concurrent edits isolated and makes item-level realtime updates
straightforward.

## Ownership

Shopping lists are a new backend business capability and should live under:

```txt
src/Domu.Api/Features/ShoppingLists/
  Domain/
  Application/
  Infrastructure/
  Interface/
```

Tests should mirror the feature:

```txt
tests/Domu.Tests/Features/ShoppingLists/
  Domain/
  Application/
  Infrastructure/
  Interface/
```

The feature may reference stable application contracts from `Households` and
`Spaces` when validating membership, Space ownership, or Item ownership. It must
not reach into another feature's infrastructure or interface.

## Domain Model

### ShoppingList

```csharp
public sealed class ShoppingList
{
    public Guid Id { get; set; }
    public Guid HouseholdId { get; set; }
    public string Name { get; set; } = null!;
    public bool IsDefault { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ArchivedAt { get; set; }
    public ICollection<ShoppingListItem> Items { get; set; } = new List<ShoppingListItem>();
}
```

### ShoppingListItem

```csharp
public sealed class ShoppingListItem
{
    public Guid Id { get; set; }
    public Guid HouseholdId { get; set; }
    public Guid ShoppingListId { get; set; }
    public string Name { get; set; } = null!;
    public string NormalizedName { get; set; } = null!;
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Note { get; set; }
    public bool Checked { get; set; }
    public DateTimeOffset? CheckedAt { get; set; }
    public Guid? CheckedByUserId { get; set; }
    public Guid? SpaceId { get; set; }
    public Guid? ItemId { get; set; }
    public Guid AddedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public decimal SortOrder { get; set; }
}
```

Keep navigation properties only where they match existing EF conventions.

## Persistence Rules

Required fields:

- `ShoppingList`: `Id`, `HouseholdId`, `Name`, `IsDefault`,
  `CreatedByUserId`, `CreatedAt`, `UpdatedAt`.
- `ShoppingListItem`: `Id`, `HouseholdId`, `ShoppingListId`, `Name`,
  `NormalizedName`, `Checked`, `AddedByUserId`, `CreatedAt`, `UpdatedAt`,
  `SortOrder`.

Indexes:

- `IX_ShoppingLists_HouseholdId`
- `IX_ShoppingLists_HouseholdId_IsDefault`
- Unique filtered index for one active default list per household:
  `HouseholdId + IsDefault` where `IsDefault = true` and `ArchivedAt is null`.
- `IX_ShoppingListItems_HouseholdId`
- `IX_ShoppingListItems_ShoppingListId`
- `IX_ShoppingListItems_ShoppingListId_Checked`
- `IX_ShoppingListItems_ShoppingListId_SortOrder`
- `IX_ShoppingListItems_SpaceId`
- `IX_ShoppingListItems_ItemId`

Do not generate migrations unless the user explicitly asks in the same turn.
Implementation may update entities, configurations, and the DbContext.

## Validation

Name:

- Required.
- Trim leading/trailing whitespace.
- Collapse repeated whitespace.
- Maximum length 120.
- Store `NormalizedName` as lowercase cleaned name.

Quantity:

- Nullable.
- Must be greater than `0` when provided.
- Decimal values are valid.

Unit:

- Nullable.
- Trimmed.
- Maximum length 32.

Note:

- Nullable.
- Trimmed.
- Maximum length 500.

References:

- `SpaceId`, when provided, must belong to the route household.
- `ItemId`, when provided, must belong to the route household.
- `ShoppingListId` must belong to the route household.
- `ShoppingListItemId` must belong to the route shopping list.

## API Contract

Use the existing controller style. Prefer one controller under the
`ShoppingLists` feature.

Default list:

```http
GET /api/households/{householdId}/shopping-list/default
```

Items:

```http
GET    /api/households/{householdId}/shopping-lists/{shoppingListId}/items
POST   /api/households/{householdId}/shopping-lists/{shoppingListId}/items
PATCH  /api/households/{householdId}/shopping-lists/{shoppingListId}/items/{itemId}
POST   /api/households/{householdId}/shopping-lists/{shoppingListId}/items/{itemId}/check
POST   /api/households/{householdId}/shopping-lists/{shoppingListId}/items/{itemId}/uncheck
DELETE /api/households/{householdId}/shopping-lists/{shoppingListId}/items/{itemId}
DELETE /api/households/{householdId}/shopping-lists/{shoppingListId}/items/checked
```

Sorting:

- Active items first by `SortOrder`, then `CreatedAt`.
- Checked items separately by `CheckedAt desc`, then `UpdatedAt desc`.

Status codes:

- `200` for reads and updates returning DTOs.
- `201` for item creation if existing API conventions use created responses.
- `204` for deletes and clear checked.
- `400` for validation failures.
- `401` for unauthenticated requests.
- `403` for authenticated non-members.
- `404` for missing lists/items within an authorized household.

## DTOs And Requests

```csharp
public sealed record ShoppingListDto(
    Guid Id,
    Guid HouseholdId,
    string Name,
    bool IsDefault,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ArchivedAt);

public sealed record ShoppingListItemDto(
    Guid Id,
    Guid HouseholdId,
    Guid ShoppingListId,
    string Name,
    string NormalizedName,
    decimal? Quantity,
    string? Unit,
    string? Note,
    bool Checked,
    DateTimeOffset? CheckedAt,
    Guid? CheckedByUserId,
    Guid? SpaceId,
    Guid? ItemId,
    Guid AddedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    decimal SortOrder);

public sealed record CreateShoppingListItemRequest(
    string Name,
    decimal? Quantity,
    string? Unit,
    string? Note,
    Guid? SpaceId,
    Guid? ItemId);

public sealed record UpdateShoppingListItemRequest(
    string? Name,
    decimal? Quantity,
    string? Unit,
    string? Note,
    Guid? SpaceId,
    Guid? ItemId,
    decimal? SortOrder);
```

## Application Behavior

Application services should expose use-case level operations rather than a
generic CRUD service:

- Get or create default list.
- Get list items.
- Create item.
- Update item.
- Check item.
- Uncheck item.
- Delete item.
- Clear checked items.

Each operation must:

- Use the current authenticated user.
- Verify household membership.
- Validate route ownership.
- Validate optional Space and Item references.
- Set timestamps and user metadata consistently.
- Save changes before publishing realtime events.

Default list creation should be idempotent. If two requests race to create the
default list, handle the unique constraint path by returning the existing list.

## Realtime

Use existing realtime infrastructure if present. If none exists, add only a small
application abstraction so the feature can publish events without coupling to a
transport.

Event payload:

```txt
householdId
shoppingListId
itemId when relevant
eventType: item-created | item-updated | item-checked | item-unchecked | item-deleted | checked-cleared
occurredAt
```

Send events only after the database mutation succeeds. For V1, the frontend may
simply refetch list items when a relevant event arrives.

## Frontend Shape

Use the existing frontend project conventions. Suggested feature structure:

```txt
src/features/shopping-list/
  api/
    shoppingListApi.ts
    shoppingListContracts.ts
  hooks/
    useDefaultShoppingList.ts
    useShoppingListItems.ts
    useShoppingListMutations.ts
    useShoppingListRealtime.ts
  screens/
    ShoppingListScreen.tsx
  components/
    AddShoppingListItemInput.tsx
    ShoppingListItemRow.tsx
    ShoppingListItemEditorSheet.tsx
    ShoppingListEmptyState.tsx
  utils/
    shoppingListText.ts
    shoppingListValidation.ts
    shoppingListFormatting.ts
  types/
    shoppingListTypes.ts
```

Core frontend behavior:

- Load the default list for the selected household.
- Load items for the default list.
- Show loading, error, empty, active, and checked states.
- Add item with only a name.
- Check, uncheck, delete, and clear checked.
- Edit name, quantity, unit, note, and Space assignment.
- Add from Space and Item screens.
- Subscribe to household/list events and refetch items on relevant events.
- Reset data correctly when switching households.

## Implementation Phases

### Phase 1: Backend Domain And Persistence

Scope:

- Add `Features/ShoppingLists` domain entities.
- Add EF configurations.
- Add DbContext sets and relationships.
- Add repository/query ports and infrastructure implementations if that matches
  existing feature patterns.
- Do not create EF migrations unless explicitly requested.

Acceptance:

- Project builds.
- Entity relationships and indexes are configured.
- Tests cover domain invariants that have meaningful behavior.

### Phase 2: Backend Application Use Cases

Scope:

- Add DTOs, request/input models as appropriate, and mapping.
- Add text normalization helper.
- Implement get/create default list and item mutation use cases.
- Enforce membership and ownership validation.

Acceptance:

- Use cases return expected DTOs.
- Validation and authorization paths are covered by application tests.
- Default list creation is idempotent under duplicate creation attempts.

### Phase 3: Backend Interface

Scope:

- Add controller and all planned endpoints.
- Match existing response and error handling conventions.
- Wire dependencies in composition root.

Acceptance:

- Endpoints use authenticated user context.
- Non-members cannot access or mutate lists.
- Interface tests cover route behavior where existing test style supports it.

### Phase 4: Backend Realtime

Scope:

- Add event payload and publisher abstraction only if needed.
- Publish after create, update, check, uncheck, delete, and clear checked.

Acceptance:

- Events are household scoped.
- Events are emitted only after successful persistence.
- If realtime infrastructure is absent, the abstraction is tested without
  overbuilding transport details.

### Phase 5: Frontend Contracts And Data Hooks

Scope:

- Add TypeScript contracts matching backend DTOs and requests.
- Add API client methods.
- Add query and mutation hooks.
- Add utility functions for text cleaning, validation, and quantity formatting.

Acceptance:

- Hooks expose loading and error state.
- Mutations invalidate or refresh item queries.
- Utility functions are tested.

### Phase 6: Frontend Main Screen

Scope:

- Add route and `ShoppingListScreen`.
- Render loading, error, empty, active, and checked sections.
- Add quick-add, check, uncheck, delete, and clear checked behavior.

Acceptance:

- User can perform the core list workflow from the screen.
- UI follows existing navigation, styling, and toast conventions.
- Switching households shows the correct list.

### Phase 7: Frontend Item Editing And Integrations

Scope:

- Add item editor sheet.
- Edit name, quantity, unit, note, and Space assignment.
- Add from Space screen.
- Add existing Item to shopping list from Item details or equivalent screen.

Acceptance:

- Validation errors are visible.
- Created list items include expected `spaceId` and `itemId`.
- Integration points use existing navigation and mutation conventions.

### Phase 8: Frontend Realtime And Polish

Scope:

- Subscribe to shopping list events for the selected household/list.
- Refetch items on relevant events.
- Clean up subscriptions on unmount or household switch.
- Add component, hook, and integration tests where project tooling supports them.

Acceptance:

- Changes from another household member refresh the list.
- Duplicate subscriptions are avoided.
- Loading, empty, and error states behave on slow or failed network requests.

## Recommended Execution Order

```txt
1. Backend Phase 1: Domain and persistence
2. Backend Phase 2: Application use cases
3. Backend Phase 3: Interface
4. Backend tests for Phases 1-3
5. Frontend Phase 5: Contracts and data hooks
6. Frontend Phase 6: Main screen
7. Frontend Phase 7: Item editing and integrations
8. Backend Phase 4: Realtime
9. Frontend Phase 8: Realtime and polish
10. End-to-end verification
```

Realtime is intentionally late. The feature should work correctly through normal
API request/response behavior before live updates are added.

## Final Acceptance Criteria

The feature is complete when:

- A household member can open the default household shopping list.
- A household member can add an item with only a name.
- A household member can edit name, quantity, unit, note, and Space.
- A household member can check, uncheck, delete, and clear checked items.
- Checked items appear separately from active items.
- A household member can add from a Space.
- A household member can add an existing household Item.
- Non-members cannot access or mutate shopping lists.
- Linked Spaces and Items must belong to the same household.
- Shopping list item changes sync near-instantly to other household members.
- Switching households shows the correct shopping list.
- Backend tests cover validation, use cases, authorization, persistence, and API
  behavior.
- Frontend tests cover utilities, hooks, and core UI behavior.

## Verification Commands

From the repo root, use the backend commands documented in
`backend/dotnet/AGENTS.md`.

Preferred backend build:

```powershell
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'; $env:DOTNET_CLI_TELEMETRY_OPTOUT='1'; $env:DOTNET_NOLOGO='1'; $env:DOTNET_CLI_HOME='backend/dotnet/src/Domu.Api/.dotnet'; dotnet build 'backend/dotnet/Domu.sln' -v minimal
```

Preferred backend test:

```powershell
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'; $env:DOTNET_CLI_TELEMETRY_OPTOUT='1'; $env:DOTNET_NOLOGO='1'; $env:DOTNET_CLI_HOME='backend/dotnet/src/Domu.Api/.dotnet'; dotnet test 'backend/dotnet/tests/Domu.Tests/Domu.Tests.csproj' --no-restore -v minimal
```
