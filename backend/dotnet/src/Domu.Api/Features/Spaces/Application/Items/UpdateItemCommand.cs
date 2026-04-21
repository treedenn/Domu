using Domu.Api.Features.Spaces.Application.Items.Contracts;

namespace Domu.Api.Features.Spaces.Application.Items;

public sealed record UpdateItemCommand(
    Guid ItemId,
    Guid SpaceId,
    string Name,
    string? Category,
    string? Barcode,
    IReadOnlyCollection<ItemEntryDraft>? Entries = null);
