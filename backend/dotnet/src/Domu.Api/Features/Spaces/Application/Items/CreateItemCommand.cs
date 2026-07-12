using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Spaces.Application.Items.Contracts;

namespace Domu.Api.Features.Spaces.Application.Items;

public sealed record CreateItemCommand(
    DomuActor Actor,
    Guid HouseholdId,
    Guid SpaceId,
    string Name,
    string? Category,
    string? Barcode,
    IReadOnlyCollection<ItemEntryDraft>? Entries = null);
