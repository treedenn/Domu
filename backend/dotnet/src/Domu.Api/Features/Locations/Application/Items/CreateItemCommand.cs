using Domu.Api.Features.Locations.Application.Items.Contracts;

namespace Domu.Api.Features.Locations.Application.Items;

public sealed record CreateItemCommand(
    Guid LocationId,
    string Name,
    string? Category,
    string? Barcode,
    IReadOnlyCollection<ItemEntryDraft>? Entries = null);
