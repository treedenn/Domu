using Domu.Api.Features.Spaces.Application.Items.Contracts;

namespace Domu.Api.Features.Spaces.Application.Items;

public sealed record ReplaceItemEntriesCommand(
    Guid UserId,
    Guid HouseholdId,
    Guid SpaceId,
    Guid ItemId,
    IReadOnlyCollection<ItemEntryDraft> Entries);
