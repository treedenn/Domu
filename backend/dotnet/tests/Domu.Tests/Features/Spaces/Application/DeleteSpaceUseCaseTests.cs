using Domu.Api.Features.Spaces.Application.Spaces;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;
using Domu.Api.Features.Spaces.Domain.Spaces;

namespace Domu.Tests.Features.Spaces.Application;

public sealed class DeleteSpaceUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DeletesExistingSpace()
    {
        var space = new Space(Guid.NewGuid(), "Pantry", Guid.NewGuid());
        var repository = new FakeSpaceRepository(space);
        var useCase = new DeleteSpaceUseCase(repository);

        await useCase.ExecuteAsync(new DeleteSpaceCommand(space.Id), CancellationToken.None);

        Assert.Empty(repository.StoredSpaces);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSpaceDoesNotExist_Throws()
    {
        var repository = new FakeSpaceRepository();
        var useCase = new DeleteSpaceUseCase(repository);

        var action = () => useCase.ExecuteAsync(new DeleteSpaceCommand(Guid.NewGuid()), CancellationToken.None);

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
            var removed = StoredSpaces.RemoveAll(space => space.Id == spaceId);
            if (removed == 0)
                throw new KeyNotFoundException($"Space '{spaceId}' was not found.");

            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }
}
