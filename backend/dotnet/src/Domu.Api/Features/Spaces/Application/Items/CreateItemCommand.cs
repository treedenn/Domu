using Domu.Api.Features.Spaces.Application.Items.Contracts;

namespace Domu.Api.Features.Spaces.Application.Items;

public sealed record CreateItemCommand(
    Guid SpaceId,
    string Name,
    string? Category,
    string? Barcode,
    IReadOnlyCollection<ItemEntryDraft>? Entries = null);
