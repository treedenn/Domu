using Domu.Api.Features.Spaces.Application.Spaces;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;
using Domu.Api.Features.Spaces.Domain.Spaces;

namespace Domu.Tests.Features.Spaces.Application;

public sealed class UpdateSpaceUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_UpdatesExistingSpace()
    {
        var space = new Space(Guid.NewGuid(), "Pantry", Guid.NewGuid());
        var repository = new FakeSpaceRepository(space);
        var useCase = new UpdateSpaceUseCase(repository);
        var parentId = Guid.NewGuid();

        var result = await useCase.ExecuteAsync(
            new UpdateSpaceCommand(space.Id, "Kitchen Pantry", "Updated description", parentId),
            CancellationToken.None);

        Assert.Equal("Kitchen Pantry", result.Name);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal(parentId, result.ParentId);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSpaceDoesNotExist_Throws()
    {
        var repository = new FakeSpaceRepository();
        var useCase = new UpdateSpaceUseCase(repository);

        var action = () => useCase.ExecuteAsync(
            new UpdateSpaceCommand(Guid.NewGuid(), "Pantry", null, null),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    private sealed class FakeSpaceRepository(params Space[] seededSpaces) : ISpaceRepository
    {
        public List<Space> StoredSpaces { get; } = seededSpaces.ToList();
        public int SaveChangesCalls { get; private set; }

        public Task<Space?> GetByIdAsync(Guid spaceId, CancellationToken cancellationToken)
        {
            return Task.FromResult(StoredSpaces.SingleOrDefault(space => space.Id == spaceId));
        }

        public Task AddAsync(Space space, CancellationToken cancellationToken)
        {
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
