using Domu.Api.Features.Locations.Application.Locations;
using Domu.Api.Features.Locations.Application.Locations.Ports;
using Domu.Api.Features.Locations.Domain.Locations;

namespace Domu.Tests.Features.Locations.Application;

public sealed class CreateLocationUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesLocationAndPersistsIt()
    {
        var repository = new FakeLocationRepository();
        var useCase = new CreateLocationUseCase(repository);
        var ownerId = Guid.NewGuid();
        var parentId = Guid.NewGuid();

        var result = await useCase.ExecuteAsync(
            new CreateLocationCommand(ownerId, "Pantry", "Food storage", parentId),
            CancellationToken.None);

        Assert.Equal(ownerId, result.OwnerId);
        Assert.Equal(parentId, result.ParentId);
        Assert.Equal("Pantry", result.Name);
        Assert.Equal("Food storage", result.Description);
        Assert.Equal(1, repository.AddCalls);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    private sealed class FakeLocationRepository : ILocationRepository
    {
        public List<Location> StoredLocations { get; } = [];
        public int AddCalls { get; private set; }
        public int SaveChangesCalls { get; private set; }

        public Task<Location?> GetByIdAsync(Guid locationId, CancellationToken cancellationToken)
        {
            return Task.FromResult(StoredLocations.SingleOrDefault(location => location.Id == locationId));
        }

        public Task AddAsync(Location location, CancellationToken cancellationToken)
        {
            AddCalls++;
            StoredLocations.Add(location);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Location location, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid locationId, CancellationToken cancellationToken)
        {
            StoredLocations.RemoveAll(location => location.Id == locationId);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }
}
