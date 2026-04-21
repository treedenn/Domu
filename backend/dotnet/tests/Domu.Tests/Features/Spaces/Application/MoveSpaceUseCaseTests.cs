using Domu.Api.Features.Spaces.Application.Spaces;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;
using Domu.Api.Features.Spaces.Domain.Spaces;

namespace Domu.Tests.Features.Spaces.Application;

public sealed class MoveSpaceUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_UpdatesParentId()
    {
        var space = new Space(Guid.NewGuid(), "Pantry", Guid.NewGuid());
        var repository = new FakeSpaceRepository(space);
        var useCase = new MoveSpaceUseCase(repository);
        var parentId = Guid.NewGuid();

        var result = await useCase.ExecuteAsync(new MoveSpaceCommand(space.Id, parentId), CancellationToken.None);

        Assert.Equal(parentId, result.ParentId);
        Assert.Equal(1, repository.SaveChangesCalls);
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
