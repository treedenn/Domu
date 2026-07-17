using Domu.Api.Features.Spaces.Application.Items.Contracts;

namespace Domu.Api.Features.Spaces.Application.Search.Contracts;

public sealed record ItemSearchResultView(
    Guid Id,
    Guid SpaceId,
    string Name,
    string? Category,
    string? Barcode,
    int TotalCount,
    IReadOnlyList<ItemEntryView> Entries);
