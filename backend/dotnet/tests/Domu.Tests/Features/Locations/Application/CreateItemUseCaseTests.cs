using Domu.Api.Features.Locations.Application.Items;
using Domu.Api.Features.Locations.Application.Items.Contracts;
using Domu.Api.Features.Locations.Application.Items.Ports;
using Domu.Api.Features.Locations.Domain.Items;

namespace Domu.Tests.Features.Locations.Application;

public sealed class CreateItemUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesItemAndPersistsAggregate()
    {
        var repository = new FakeItemRepository();
        var useCase = new CreateItemUseCase(repository);
        var locationId = Guid.NewGuid();

        var result = await useCase.ExecuteAsync(
            new CreateItemCommand(
                locationId,
                "Pasta",
                "Food",
                "123456789",
                [new ItemEntryDraft(null, 2, ConsumableState.Unopened, null, null)]),
            CancellationToken.None);

        Assert.Equal(locationId, result.LocationId);
        Assert.Equal("Pasta", result.Name);
        Assert.Equal("Food", result.Category);
        Assert.Equal("123456789", result.Barcode);
        Assert.Equal(2, result.TotalQuantity);
        Assert.Single(result.Entries);
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

        public Task<IReadOnlyList<Item>> GetByLocationAsync(Guid locationId, CancellationToken cancellationToken)
        {
            IReadOnlyList<Item> items = StoredItems.Where(item => item.LocationId == locationId).ToArray();
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
            var index = StoredItems.FindIndex(existingItem => existingItem.Id == item.Id);
            if (index >= 0)
                StoredItems[index] = item;

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
