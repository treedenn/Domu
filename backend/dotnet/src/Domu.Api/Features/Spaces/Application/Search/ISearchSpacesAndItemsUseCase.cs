using Domu.Api.Features.Spaces.Application.Search.Contracts;

namespace Domu.Api.Features.Spaces.Application.Search;

public interface ISearchSpacesAndItemsUseCase
{
    Task<SearchResultsView> ExecuteAsync(SearchSpacesAndItemsQuery query, CancellationToken cancellationToken);
}
