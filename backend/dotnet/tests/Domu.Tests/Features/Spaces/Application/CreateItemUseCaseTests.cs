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
        var useCase = new CreateItemUseCase(repository, new FakeSpaceAccessService());
        var spaceId = Guid.NewGuid();

        var result = await useCase.ExecuteAsync(
            new CreateItemCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                spaceId,
                "Milk",
                "Dairy",
                "123",
                [new ItemEntryDraft(null, 2, ConsumableState.Unopened, null, null)]),
            CancellationToken.None);

        Assert.Equal("Milk", result.Name);
        Assert.Equal("Dairy", result.Category);
        Assert.Equal("123", result.Barcode);
        Assert.Equal(spaceId, result.SpaceId);
        Assert.Equal(1, repository.AddCalls);
        Assert.Equal(1, repository.SaveChangesCalls);
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
}
