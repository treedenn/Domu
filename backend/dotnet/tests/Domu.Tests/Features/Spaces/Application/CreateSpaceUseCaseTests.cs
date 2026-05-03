using Domu.Api.Features.Spaces.Application.Spaces;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;
using Domu.Api.Features.Spaces.Domain.Spaces;

namespace Domu.Tests.Features.Spaces.Application;

public sealed class CreateSpaceUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesSpaceAndPersistsIt()
    {
        var repository = new FakeSpaceRepository();
        var useCase = new CreateSpaceUseCase(repository, new FakeSpaceAccessService());
        var householdId = Guid.NewGuid();
        var parentId = Guid.NewGuid();

        var result = await useCase.ExecuteAsync(
            new CreateSpaceCommand(Guid.NewGuid(), householdId, "Pantry", "Food storage", parentId),
            CancellationToken.None);

        Assert.Equal(householdId, result.HouseholdId);
        Assert.Equal(parentId, result.ParentId);
        Assert.Equal("Pantry", result.Name);
        Assert.Equal("Food storage", result.Description);
        Assert.Equal(1, repository.AddCalls);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    private sealed class FakeSpaceRepository : ISpaceRepository
    {
        public List<Space> StoredSpaces { get; } = [];
        public int AddCalls { get; private set; }
        public int SaveChangesCalls { get; private set; }

        public Task<Space?> GetByIdAsync(Guid spaceId, CancellationToken cancellationToken)
        {
            return Task.FromResult(StoredSpaces.SingleOrDefault(space => space.Id == spaceId));
        }

        public Task AddAsync(Space space, CancellationToken cancellationToken)
        {
            AddCalls++;
            StoredSpaces.Add(space);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Space space, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid spaceId, CancellationToken cancellationToken)
        {
            StoredSpaces.RemoveAll(space => space.Id == spaceId);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }
}
