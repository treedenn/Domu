using Domu.Api.Features.Locations.Application.Items;
using Domu.Api.Features.Locations.Application.Items.Contracts;
using Domu.Api.Features.Locations.Application.Items.Ports;
using Domu.Api.Features.Locations.Domain.Items;

namespace Domu.Tests.Features.Locations.Application;

public sealed class UpdateItemUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_UpdatesExistingItemAndReplacesEntries()
    {
        var item = new Item(Guid.NewGuid(), "Rice", Guid.NewGuid());
        var existingEntry = new ItemEntry(Guid.NewGuid(), item.Id);
        existingEntry.SetQuantity(1);
        item.AddEntry(existingEntry);

        var repository = new FakeItemRepository(item);
        var useCase = new UpdateItemUseCase(repository);
        var newLocationId = Guid.NewGuid();

        var result = await useCase.ExecuteAsync(
            new UpdateItemCommand(
                item.Id,
                newLocationId,
                "Brown Rice",
                "Dry Food",
                "987654321",
                [new ItemEntryDraft(null, 3, ConsumableState.Opened, null, null)]),
            CancellationToken.None);

        Assert.Equal(newLocationId, result.LocationId);
        Assert.Equal("Brown Rice", result.Name);
        Assert.Equal("Dry Food", result.Category);
        Assert.Equal("987654321", result.Barcode);
        Assert.Equal(3, result.TotalQuantity);
        Assert.Single(result.Entries);
        Assert.Equal(1, repository.UpdateCalls);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_WhenItemDoesNotExist_Throws()
    {
        var repository = new FakeItemRepository();
        var useCase = new UpdateItemUseCase(repository);

        var action = () => useCase.ExecuteAsync(
            new UpdateItemCommand(Guid.NewGuid(), Guid.NewGuid(), "Beans", null, null),
            CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(action);
    }

    private sealed class FakeItemRepository(params Item[] seededItems) : IItemRepository
    {
        public List<Item> StoredItems { get; } = seededItems.ToList();
        public int UpdateCalls { get; private set; }
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
            StoredItems.Add(item);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Item item, CancellationToken cancellationToken)
        {
            UpdateCalls++;
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
