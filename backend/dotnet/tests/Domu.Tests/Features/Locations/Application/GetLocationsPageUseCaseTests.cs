using Domu.Api.Features.Locations.Application.Locations;
using Domu.Api.Features.Locations.Application.Locations.Contracts;
using Domu.Api.Features.Locations.Application.Locations.Ports;

namespace Domu.Tests.Features.Locations.Application;

public sealed class GetLocationsPageUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsProjectedPage()
    {
        var expectedPage = new LocationPage(
            [
                new LocationView(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    null,
                    "Pantry",
                    "Food storage",
                    2,
                    [new LocationItemView(Guid.NewGuid(), Guid.NewGuid(), "Pasta", "Food", "123", 4)],
                    1,
                    [new LocationChildView(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Shelf", null)])
            ],
            2,
            10,
            17);
        var queryService = new FakeLocationQueryService(expectedPage);
        var useCase = new GetLocationsPageUseCase(queryService);
        var query = new GetLocationsPageQuery(
            Guid.NewGuid(),
            null,
            2,
            10,
            LocationItemsProjection.Count | LocationItemsProjection.Data,
            LocationChildrenProjection.Count | LocationChildrenProjection.Data);

        var result = await useCase.ExecuteAsync(query, CancellationToken.None);

        Assert.Equal(expectedPage, result);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPageSize_Throws()
    {
        var queryService = new FakeLocationQueryService(new LocationPage([], 1, 1, 0));
        var useCase = new GetLocationsPageUseCase(queryService);

        var action = () => useCase.ExecuteAsync(
            new GetLocationsPageQuery(Guid.NewGuid(), null, PageNumber: 1, PageSize: 0),
            CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(action);
    }

    private sealed class FakeLocationQueryService(LocationPage page) : ILocationQueryService
    {
        public Task<LocationPage> GetPageAsync(GetLocationsPageQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult(page);
        }
    }
}
