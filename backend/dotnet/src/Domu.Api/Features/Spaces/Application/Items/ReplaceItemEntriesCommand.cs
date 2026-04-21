using Domu.Api.Features.Spaces.Application.Items.Contracts;

namespace Domu.Api.Features.Spaces.Application.Items;

public sealed record ReplaceItemEntriesCommand(Guid ItemId, IReadOnlyCollection<ItemEntryDraft> Entries);
