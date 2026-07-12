using Domu.Api.Features.Auth.Domain;

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
        var useCase = new SearchSpacesAndItemsUseCase(service, new FakeHouseholdAccessService());
        var query = new SearchSpacesAndItemsQuery(new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel), Guid.NewGuid(), "milk", null, 20);

        await useCase.ExecuteAsync(query, CancellationToken.None);

        Assert.Equal(query, service.LastQuery);
    }

    [Fact]
    public async Task ExecuteAsync_WithLimitAboveMaximum_Throws()
    {
        var useCase = new SearchSpacesAndItemsUseCase(new FakeSearchService(), new FakeHouseholdAccessService());

        var action = () => useCase.ExecuteAsync(
            new SearchSpacesAndItemsQuery(new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel), Guid.NewGuid(), "milk", null, 101),
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
