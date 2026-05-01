using Domu.Api.Features.Spaces.Application.Search;
using Domu.Api.Features.Spaces.Application.Search.Contracts;
using Domu.Api.Features.Spaces.Application.Search.Ports;

namespace Domu.Tests.Features.Spaces.Application;

public sealed class SearchSpacesAndItemsUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidQuery_DelegatesToSearchService()
    {
        var service = new FakeSearchService();
        var useCase = new SearchSpacesAndItemsUseCase(service);
        var query = new SearchSpacesAndItemsQuery(Guid.NewGuid(), "milk", null, 20);

        await useCase.ExecuteAsync(query, CancellationToken.None);

        Assert.Equal(query, service.LastQuery);
    }

    [Fact]
    public async Task ExecuteAsync_WithLimitAboveMaximum_Throws()
    {
        var useCase = new SearchSpacesAndItemsUseCase(new FakeSearchService());

        var action = () => useCase.ExecuteAsync(
            new SearchSpacesAndItemsQuery(Guid.NewGuid(), "milk", null, 101),
            CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(action);
    }

    private sealed class FakeSearchService : ISpacesAndItemsSearchService
    {
        public SearchSpacesAndItemsQuery? LastQuery { get; private set; }

        public Task<SearchResultsView> SearchAsync(
            SearchSpacesAndItemsQuery query,
            CancellationToken cancellationToken)
        {
            LastQuery = query;
            return Task.FromResult(new SearchResultsView([], []));
        }
    }
}
