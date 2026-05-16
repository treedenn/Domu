using Domu.Api.Features.Events.Application;
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
        var eventRecorder = new FakeUserEventRecorder();
        var useCase = new CreateItemUseCase(repository, new FakeSpaceAccessService(), eventRecorder);
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var spaceId = Guid.NewGuid();

        var result = await useCase.ExecuteAsync(
            new CreateItemCommand(
                userId,
                householdId,
                spaceId,
                "Milk",
                "Dairy",
                "123",
                [new ItemEntryDraft(null, 2, 2, ItemUnit.Piece, ItemContainerType.Unspecified, ConsumableState.Unopened, null, null)]),
            CancellationToken.None);

        Assert.Equal("Milk", result.Name);
        Assert.Equal("Dairy", result.Category);
        Assert.Equal("123", result.Barcode);
        Assert.Equal(spaceId, result.SpaceId);
        Assert.Equal(1, repository.AddCalls);
        Assert.Equal(1, repository.SaveChangesCalls);
        var userEvent = Assert.Single(eventRecorder.Events);
        Assert.Equal(userId, userEvent.ActorUserId);
        Assert.Equal(householdId, userEvent.HouseholdId);
        Assert.Equal(UserEventActions.ItemCreated, userEvent.Action);
        Assert.Equal(UserEventTargetTypes.Item, userEvent.TargetType);
        Assert.Equal(result.Id, userEvent.TargetId);
        Assert.Equal(spaceId, userEvent.Metadata["spaceId"]);
        Assert.Equal("Milk", userEvent.Metadata["name"]);
        Assert.Equal("Dairy", userEvent.Metadata["category"]);
        Assert.Equal("123", userEvent.Metadata["barcode"]);
        Assert.Equal(1, userEvent.Metadata["entryCount"]);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAccessFails_DoesNotRecordEvent()
    {
        var repository = new FakeItemRepository();
        var eventRecorder = new FakeUserEventRecorder();
        var accessService = new FakeSpaceAccessService { DenyAccess = true };
        var useCase = new CreateItemUseCase(repository, accessService, eventRecorder);

        var action = () => useCase.ExecuteAsync(
            new CreateItemCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Milk",
                null,
                null,
                null),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
        Assert.Empty(eventRecorder.Events);
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

    private sealed class FakeUserEventRecorder : IUserEventRecorder
    {
        public List<RecordedEvent> Events { get; } = [];

        public Task RecordAsync(
            Guid actorUserId,
            string action,
            string targetType,
            Guid? targetId,
            Guid? householdId,
            EventMetadata? metadata,
            CancellationToken cancellationToken)
        {
            Events.Add(new RecordedEvent(
                actorUserId,
                action,
                targetType,
                targetId,
                householdId,
                metadata ?? EventMetadata.Empty()));
            return Task.CompletedTask;
        }
    }

    private sealed record RecordedEvent(
        Guid ActorUserId,
        string Action,
        string TargetType,
        Guid? TargetId,
        Guid? HouseholdId,
        EventMetadata Metadata);
}
