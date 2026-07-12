namespace Domu.Api.Features.Spaces.Application.Search.Contracts;

public sealed record SearchResultsView(
    IReadOnlyList<SpaceSearchResultView> Spaces,
    IReadOnlyList<ItemSearchResultView> Items);