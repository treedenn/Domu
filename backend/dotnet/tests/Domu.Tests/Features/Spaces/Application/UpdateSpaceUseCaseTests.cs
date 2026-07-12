using Domu.Api.Features.Auth.Domain;

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
        space.MoveTo(Guid.NewGuid());
        var repository = new FakeSpaceRepository(space);
        var useCase = new UpdateSpaceUseCase(repository, new FakeSpaceAccessService());
        var originalParentId = space.ParentId;

        var result = await useCase.ExecuteAsync(
            new UpdateSpaceCommand(new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel), space.HouseholdId, space.Id, "Kitchen Pantry", "Updated description"),
            CancellationToken.None);

        Assert.Equal("Kitchen Pantry", result.Name);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal(originalParentId, result.ParentId);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSpaceDoesNotExist_Throws()
    {
        var repository = new FakeSpaceRepository();
        var useCase = new UpdateSpaceUseCase(repository, new FakeSpaceAccessService());

        var action = () => useCase.ExecuteAsync(
            new UpdateSpaceCommand(new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel), Guid.NewGuid(), Guid.NewGuid(), "Pantry", null),
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
