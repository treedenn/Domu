using Domu.Api.Features.Auth.Domain;

using Domu.Api.Features.Spaces.Application.Items.Contracts;

namespace Domu.Api.Features.Spaces.Application.Items;

public sealed record ReplaceItemEntriesCommand(
    DomuActor Actor,
    Guid HouseholdId,
    Guid SpaceId,
    Guid ItemId,
    IReadOnlyCollection<ItemEntryDraft> Entries);
