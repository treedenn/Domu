using Domu.Api.Features.Spaces.Application.Spaces;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;
using Domu.Api.Features.Spaces.Domain.Spaces;

namespace Domu.Tests.Features.Spaces.Application;

public sealed class GetSpaceUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsExistingSpaceInHousehold()
    {
        var householdId = Guid.NewGuid();
        var space = new Space(Guid.NewGuid(), "Pantry", householdId);
        space.Describe("Dry goods");
        space.MoveTo(Guid.NewGuid());
        var repository = new FakeSpaceRepository(space);
        var useCase = new GetSpaceUseCase(repository, new FakeSpaceAccessService());

        var result = await useCase.ExecuteAsync(
            new GetSpaceQuery(Guid.NewGuid(), householdId, space.Id),
            CancellationToken.None);

        Assert.Equal(space.Id, result.Id);
        Assert.Equal(householdId, result.HouseholdId);
        Assert.Equal(space.ParentId, result.ParentId);
        Assert.Equal("Pantry", result.Name);
        Assert.Equal("Dry goods", result.Description);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSpaceDoesNotExist_Throws()
    {
        var repository = new FakeSpaceRepository();
        var useCase = new GetSpaceUseCase(repository, new FakeSpaceAccessService());

        var action = () => useCase.ExecuteAsync(
            new GetSpaceQuery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSpaceBelongsToAnotherHousehold_Throws()
    {
        var space = new Space(Guid.NewGuid(), "Pantry", Guid.NewGuid());
        var repository = new FakeSpaceRepository(space);
        var useCase = new GetSpaceUseCase(repository, new FakeSpaceAccessService { DenyAccess = true });

        var action = () => useCase.ExecuteAsync(
            new GetSpaceQuery(Guid.NewGuid(), Guid.NewGuid(), space.Id),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    private sealed class FakeSpaceRepository(params Space[] seededSpaces) : ISpaceRepository
    {
        private readonly List<Space> _storedSpaces = seededSpaces.ToList();

        public Task<Space?> GetByIdAsync(Guid spaceId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_storedSpaces.SingleOrDefault(space => space.Id == spaceId));
        }

        public Task AddAsync(Space space, CancellationToken cancellationToken)
        {
            _storedSpaces.Add(space);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Space space, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid spaceId, CancellationToken cancellationToken)
        {
            _storedSpaces.RemoveAll(space => space.Id == spaceId);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
