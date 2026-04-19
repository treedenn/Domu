using Domu.Api.Features.Locations.Application.Items.Contracts;

namespace Domu.Api.Features.Locations.Application.Items;

public sealed record UpdateItemCommand(
    Guid ItemId,
    Guid LocationId,
    string Name,
    string? Category,
    string? Barcode,
    IReadOnlyCollection<ItemEntryDraft>? Entries = null);
