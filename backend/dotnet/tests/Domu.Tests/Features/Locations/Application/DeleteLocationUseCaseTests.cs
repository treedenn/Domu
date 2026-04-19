using Domu.Api.Features.Locations.Application.Locations;
using Domu.Api.Features.Locations.Application.Locations.Ports;
using Domu.Api.Features.Locations.Domain.Locations;

namespace Domu.Tests.Features.Locations.Application;

public sealed class DeleteLocationUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DeletesExistingLocation()
    {
        var location = new Location(Guid.NewGuid(), "Pantry", Guid.NewGuid());
        var repository = new FakeLocationRepository(location);
        var useCase = new DeleteLocationUseCase(repository);

        await useCase.ExecuteAsync(new DeleteLocationCommand(location.Id), CancellationToken.None);

        Assert.Empty(repository.StoredLocations);
        Assert.Equal(1, repository.DeleteCalls);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLocationDoesNotExist_Throws()
    {
        var repository = new FakeLocationRepository();
        var useCase = new DeleteLocationUseCase(repository);

        var action = () => useCase.ExecuteAsync(new DeleteLocationCommand(Guid.NewGuid()), CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    private sealed class FakeLocationRepository(params Location[] seededLocations) : ILocationRepository
    {
        public List<Location> StoredLocations { get; } = seededLocations.ToList();
        public int DeleteCalls { get; private set; }
        public int SaveChangesCalls { get; private set; }

        public Task<Location?> GetByIdAsync(Guid locationId, CancellationToken cancellationToken)
        {
            return Task.FromResult(StoredLocations.SingleOrDefault(location => location.Id == locationId));
        }

        public Task AddAsync(Location location, CancellationToken cancellationToken)
        {
            StoredLocations.Add(location);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Location location, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid locationId, CancellationToken cancellationToken)
        {
            var removed = StoredLocations.RemoveAll(location => location.Id == locationId);
            if (removed == 0)
                throw new KeyNotFoundException($"Location '{locationId}' was not found.");

            DeleteCalls++;
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }
}
