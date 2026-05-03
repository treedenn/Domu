using Domu.Api.Features.Spaces.Application.Items;
using Domu.Api.Features.Spaces.Application.Items.Ports;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Tests.Features.Spaces.Application;

public sealed class DeleteItemUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DeletesExistingItem()
    {
        var item = new Item(Guid.NewGuid(), "Milk", Guid.NewGuid());
        var repository = new FakeItemRepository(item);
        var useCase = new DeleteItemUseCase(repository, new FakeSpaceAccessService());

        await useCase.ExecuteAsync(
            new DeleteItemCommand(Guid.NewGuid(), Guid.NewGuid(), item.SpaceId, item.Id),
            CancellationToken.None);

        Assert.Empty(repository.StoredItems);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenItemDoesNotExist_Throws()
    {
        var repository = new FakeItemRepository();
        var useCase = new DeleteItemUseCase(repository, new FakeSpaceAccessService());

        var action = () => useCase.ExecuteAsync(
            new DeleteItemCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
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
            var removed = StoredItems.RemoveAll(item => item.Id == itemId);
            if (removed == 0)
                throw new KeyNotFoundException($"Item '{itemId}' was not found.");

            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;
            return Task.CompletedTask;
        }
    }
}
