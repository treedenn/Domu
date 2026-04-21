using Domu.Api.Features.Spaces.Application.Items;
using Domu.Api.Features.Spaces.Application.Items.Contracts;
using Domu.Api.Features.Spaces.Application.Items.Ports;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Tests.Features.Spaces.Application;

public sealed class UpdateItemUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_UpdatesExistingItem()
    {
        var item = new Item(Guid.NewGuid(), "Milk", Guid.NewGuid());
        var repository = new FakeItemRepository(item);
        var useCase = new UpdateItemUseCase(repository);
        var newSpaceId = Guid.NewGuid();

        var result = await useCase.ExecuteAsync(
            new UpdateItemCommand(
                item.Id,
                newSpaceId,
                "Skim Milk",
                "Dairy",
                "123",
                [new ItemEntryDraft(null, 3, ConsumableState.Unopened, null, null)]),
            CancellationToken.None);

        Assert.Equal("Skim Milk", result.Name);
        Assert.Equal(newSpaceId, result.SpaceId);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    private sealed class FakeItemRepository(params Item[] seededItems) : IItemRepository
    {
        public List<Item> StoredItems { get; } = seededItems.ToList();
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
}
