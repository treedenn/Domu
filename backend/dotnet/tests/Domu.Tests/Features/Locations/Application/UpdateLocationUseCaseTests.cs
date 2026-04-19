using Domu.Api.Features.Locations.Application.Locations;
using Domu.Api.Features.Locations.Application.Locations.Ports;
using Domu.Api.Features.Locations.Domain.Locations;

namespace Domu.Tests.Features.Locations.Application;

public sealed class UpdateLocationUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_UpdatesExistingLocation()
    {
        var location = new Location(Guid.NewGuid(), "Pantry", Guid.NewGuid());
        var repository = new FakeLocationRepository(location);
        var useCase = new UpdateLocationUseCase(repository);
        var parentId = Guid.NewGuid();

        var result = await useCase.ExecuteAsync(
            new UpdateLocationCommand(location.Id, "Kitchen Pantry", "Updated description", parentId),
            CancellationToken.None);

        Assert.Equal("Kitchen Pantry", result.Name);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal(parentId, result.ParentId);
        Assert.Equal(1, repository.UpdateCalls);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLocationDoesNotExist_Throws()
    {
        var repository = new FakeLocationRepository();
        var useCase = new UpdateLocationUseCase(repository);

        var action = () => useCase.ExecuteAsync(
            new UpdateLocationCommand(Guid.NewGuid(), "Pantry", null, null),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    private sealed class FakeLocationRepository(params Location[] seededLocations) : ILocationRepository
    {
        public List<Location> StoredLocations { get; } = seededLocations.ToList();
        public int UpdateCalls { get; private set; }
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
            UpdateCalls++;
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
