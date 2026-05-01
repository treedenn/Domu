using Domu.Api.Features.Spaces.Application.Search.Contracts;

namespace Domu.Api.Features.Spaces.Application.Search.Ports;

public interface ISpacesAndItemsSearchService
{
    Task<SearchResultsView> SearchAsync(SearchSpacesAndItemsQuery query, CancellationToken cancellationToken);
}
