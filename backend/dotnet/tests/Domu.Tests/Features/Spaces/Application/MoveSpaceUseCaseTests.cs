using Domu.Api.Features.Auth.Domain;
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
        var useCase = new MoveSpaceUseCase(repository, new FakeSpaceAccessService());
        var parentId = Guid.NewGuid();

        var result = await useCase.ExecuteAsync(
            new MoveSpaceCommand(new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel), space.HouseholdId, space.Id,
                parentId),
            CancellationToken.None);

        Assert.Equal(parentId, result.ParentId);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenParentIsSelf_ThrowsAndDoesNotPersist()
    {
        var space = new Space(Guid.NewGuid(), "Pantry", Guid.NewGuid());
        var repository = new FakeSpaceRepository(space);
        var useCase = new MoveSpaceUseCase(repository, new FakeSpaceAccessService());

        var action = () => useCase.ExecuteAsync(
            new MoveSpaceCommand(new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel), space.HouseholdId, space.Id,
                space.Id),
            CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentException>(action);
        Assert.Equal(0, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenParentIsDirectChild_ThrowsAndDoesNotPersist()
    {
        var householdId = Guid.NewGuid();
        var space = new Space(Guid.NewGuid(), "Home", householdId);
        var child = new Space(Guid.NewGuid(), "Kitchen", householdId);
        child.MoveTo(space.Id);
        var repository = new FakeSpaceRepository(space, child);
        var useCase = new MoveSpaceUseCase(repository, new FakeSpaceAccessService());

        var action = () => useCase.ExecuteAsync(
            new MoveSpaceCommand(new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel), householdId, space.Id, child.Id),
            CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentException>(action);
        Assert.Equal(0, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenParentIsDeepDescendant_ThrowsAndDoesNotPersist()
    {
        var householdId = Guid.NewGuid();
        var space = new Space(Guid.NewGuid(), "Home", householdId);
        var child = new Space(Guid.NewGuid(), "Kitchen", householdId);
        var grandchild = new Space(Guid.NewGuid(), "Pantry", householdId);
        child.MoveTo(space.Id);
        grandchild.MoveTo(child.Id);
        var repository = new FakeSpaceRepository(space, child, grandchild);
        var useCase = new MoveSpaceUseCase(repository, new FakeSpaceAccessService());

        var action = () => useCase.ExecuteAsync(
            new MoveSpaceCommand(new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel), householdId, space.Id,
                grandchild.Id),
            CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentException>(action);
        Assert.Equal(0, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_AllowsMoveToRootAndUnrelatedSpace()
    {
        var householdId = Guid.NewGuid();
        var space = new Space(Guid.NewGuid(), "Pantry", householdId);
        var unrelatedParent = new Space(Guid.NewGuid(), "Garage", householdId);
        space.MoveTo(Guid.NewGuid());
        var repository = new FakeSpaceRepository(space, unrelatedParent);
        var useCase = new MoveSpaceUseCase(repository, new FakeSpaceAccessService());
        var actor = new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel);

        var rootResult = await useCase.ExecuteAsync(
            new MoveSpaceCommand(actor, householdId, space.Id, null), CancellationToken.None);
        var siblingResult = await useCase.ExecuteAsync(
            new MoveSpaceCommand(actor, householdId, space.Id, unrelatedParent.Id), CancellationToken.None);

        Assert.Null(rootResult.ParentId);
        Assert.Equal(unrelatedParent.Id, siblingResult.ParentId);
        Assert.Equal(2, repository.SaveChangesCalls);
    }

    private sealed class FakeSpaceRepository(params Space[] seededSpaces) : ISpaceRepository
    {
        public List<Space> StoredSpaces { get; } = seededSpaces.ToList();
        public int SaveChangesCalls { get; private set; }

        public Task<Space?> GetByIdAsync(Guid spaceId, CancellationToken cancellationToken)
        {
            return Task.FromResult(StoredSpaces.SingleOrDefault(space => space.Id == spaceId));
        }

        public Task<bool> IsDescendantAsync(Guid ancestorSpaceId, Guid candidateDescendantId, Guid householdId,
            CancellationToken cancellationToken)
        {
            var current = StoredSpaces.SingleOrDefault(space => space.Id == candidateDescendantId && space.HouseholdId == householdId);
            while (current is not null)
            {
                if (current.Id == ancestorSpaceId)
                    return Task.FromResult(true);

                current = current.ParentId is null
                    ? null
                    : StoredSpaces.SingleOrDefault(space => space.Id == current.ParentId && space.HouseholdId == householdId);
            }

            return Task.FromResult(false);
        }

        public Task<bool> HasChildrenOrItemsAsync(Guid spaceId, CancellationToken cancellationToken) => Task.FromResult(false);

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
