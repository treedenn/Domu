using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Activities.Application;
using Domu.Api.Features.Spaces.Application.Items;
using Domu.Api.Features.Spaces.Application.Items.Contracts;
using Domu.Api.Features.Spaces.Application.Items.Ports;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Tests.Features.Spaces.Application;

public sealed class CreateItemUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesItemAndPersistsIt()
    {
        var repository = new FakeItemRepository();
        var activityRecorder = new FakeHouseholdActivityRecorder();
        var useCase = new CreateItemUseCase(repository, new FakeSpaceAccessService(), activityRecorder);
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var spaceId = Guid.NewGuid();

        var result = await useCase.ExecuteAsync(
            new CreateItemCommand(
                new DomuActor(userId, DomuActorType.Zitadel),
                householdId,
                spaceId,
                "Milk",
                "Dairy",
                "123",
                [
                    new ItemEntryDraft(null, 2, 2, ItemUnit.Piece,
                        ConsumableState.Unopened, null, null)
                ]),
            CancellationToken.None);

        Assert.Equal("Milk", result.Name);
        Assert.Equal("Dairy", result.Category);
        Assert.Equal("123", result.Barcode);
        Assert.Equal(spaceId, result.SpaceId);
        Assert.Equal(1, repository.AddCalls);
        Assert.Equal(1, repository.SaveChangesCalls);
        var householdActivity = Assert.Single(activityRecorder.Activities);
        Assert.Equal(userId, householdActivity.Actor.ActorId);
        Assert.Equal(householdId, householdActivity.HouseholdId);
        Assert.Equal(HouseholdActivityActions.ItemCreated, householdActivity.Action);
        Assert.Equal(HouseholdActivityTargetTypes.Item, householdActivity.TargetType);
        Assert.Equal(result.Id, householdActivity.TargetId);
        Assert.Equal(spaceId, householdActivity.Metadata["spaceId"]);
        Assert.Equal("Milk", householdActivity.Metadata["name"]);
        Assert.Equal("Dairy", householdActivity.Metadata["category"]);
        Assert.Equal("123", householdActivity.Metadata["barcode"]);
        Assert.Equal(1, householdActivity.Metadata["entryCount"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAccessFails_DoesNotRecordActivity()
    {
        var repository = new FakeItemRepository();
        var activityRecorder = new FakeHouseholdActivityRecorder();
        var accessService = new FakeSpaceAccessService { DenyAccess = true };
        var useCase = new CreateItemUseCase(repository, accessService, activityRecorder);

        var action = () => useCase.ExecuteAsync(
            new CreateItemCommand(
                new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Milk",
                null,
                null),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
        Assert.Empty(activityRecorder.Activities);
        Assert.Equal(0, repository.SaveChangesCalls);
    }

    private sealed class FakeItemRepository : IItemRepository
    {
        public List<Item> StoredItems { get; } = [];
        public int AddCalls { get; private set; }
        public int SaveChangesCalls { get; private set; }

        public Task<Item?> GetByIdAsync(Guid itemId, CancellationToken cancellationToken)
        {
            return Task.FromResult(StoredItems.SingleOrDefault(item => item.Id == itemId));
        }

        public Task<IReadOnlyList<Item>> GetBySpaceAsync(Guid spaceId, CancellationToken cancellationToken)
        {
            IReadOnlyList<Item> items = StoredItems.Where(item => item.SpaceId == spaceId).ToArray();
            return Task.FromResult(items);
        }

        public Task AddAsync(Item item, CancellationToken cancellationToken)
        {
            AddCalls++;
            StoredItems.Add(item);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Item item, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid itemId, CancellationToken cancellationToken)
        {
            StoredItems.RemoveAll(item => item.Id == itemId);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeHouseholdActivityRecorder : IHouseholdActivityRecorder
    {
        public List<RecordedActivity> Activities { get; } = [];

        public Task RecordAsync(
            DomuActor actor,
            string action,
            string targetType,
            Guid? targetId,
            Guid householdId,
            ActivityMetadata? metadata,
            CancellationToken cancellationToken)
        {
            Activities.Add(new RecordedActivity(
                actor,
                action,
                targetType,
                targetId,
                householdId,
                metadata ?? ActivityMetadata.Empty()));
            return Task.CompletedTask;
        }
    }

    private sealed record RecordedActivity(
        DomuActor Actor,
        string Action,
        string TargetType,
        Guid? TargetId,
        Guid HouseholdId,
        ActivityMetadata Metadata);
}
