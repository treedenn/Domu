namespace Domu.Api.Features.Spaces.Interface.Items;

public sealed record ReplaceItemEntriesRequest(IReadOnlyCollection<ItemEntryRequest> Entries);