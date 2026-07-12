using Domu.Api.Features.Auth.Domain;

using Domu.Api.Features.Spaces.Application.Spaces;
using Domu.Api.Features.Spaces.Application.Spaces.Contracts;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;

namespace Domu.Tests.Features.Spaces.Application;

public sealed class GetSpacesPageUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsProjectedPage()
    {
        var expectedPage = new SpacePage(
            [
                new SpaceView(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    null,
                    "Pantry",
                    "Food storage",
                    new CollectionView<SpaceItemView>(
                        1,
                        [new SpaceItemView(Guid.NewGuid(), Guid.NewGuid(), "Pasta", "Food", "123", 4)]),
                    new CollectionView<SpaceChildView>(
                        1,
                        [new SpaceChildView(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Shelf", null)]))
            ],
            2,
            10,
            17);
        var queryService = new FakeSpaceQueryService(expectedPage);
        var useCase = new GetSpacesPageUseCase(queryService, new FakeSpaceAccessService());
        var query = new GetSpacesPageQuery(new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel),
            Guid.NewGuid(),
            null,
            2,
            10,
            SpaceItemsProjection.Count | SpaceItemsProjection.Data,
            SpaceChildrenProjection.Count | SpaceChildrenProjection.Data);

        var result = await useCase.ExecuteAsync(query, CancellationToken.None);

        Assert.Equal(expectedPage, result);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPageSize_Throws()
    {
        var queryService = new FakeSpaceQueryService(new SpacePage([], 1, 1, 0));
        var useCase = new GetSpacesPageUseCase(queryService, new FakeSpaceAccessService());

        var action = () => useCase.ExecuteAsync(
            new GetSpacesPageQuery(new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel), Guid.NewGuid(), null, PageNumber: 1, PageSize: 0),
            CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(action);
    }

    private sealed class FakeSpaceQueryService(SpacePage page) : ISpaceQueryService
    {
        public Task<SpacePage> GetPageAsync(GetSpacesPageQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult(page);
        }
    }
}
