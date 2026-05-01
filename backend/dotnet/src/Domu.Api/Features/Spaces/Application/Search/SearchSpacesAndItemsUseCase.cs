using Domu.Api.Features.Spaces.Application.Search.Contracts;
using Domu.Api.Features.Spaces.Application.Search.Ports;

namespace Domu.Api.Features.Spaces.Application.Search;

public sealed class SearchSpacesAndItemsUseCase(ISpacesAndItemsSearchService searchService)
    : ISearchSpacesAndItemsUseCase
{
    public Task<SearchResultsView> ExecuteAsync(SearchSpacesAndItemsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.ExpiringWithinDays is < 0)
            throw new ArgumentOutOfRangeException(
                nameof(query.ExpiringWithinDays),
                "Expiry filter must be greater than or equal to zero.");
        if (query.Limit <= 0)
            throw new ArgumentOutOfRangeException(nameof(query.Limit), "Limit must be greater than zero.");
        if (query.Limit > 100)
            throw new ArgumentOutOfRangeException(nameof(query.Limit), "Limit cannot be greater than 100.");

        return searchService.SearchAsync(query, cancellationToken);
    }
}
