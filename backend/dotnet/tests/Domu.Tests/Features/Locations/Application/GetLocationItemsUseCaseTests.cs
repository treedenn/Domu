using Domu.Api.Features.Locations.Application.Items;
using Domu.Api.Features.Locations.Application.Items.Ports;
using Domu.Api.Features.Locations.Domain.Items;

namespace Domu.Tests.Features.Locations.Application;

public sealed class GetLocationItemsUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsItemsForRequestedLocation()
    {
        var locationId = Guid.NewGuid();
        var matchingItem = new Item(Guid.NewGuid(), "Coffee", locationId);
        var otherItem = new Item(Guid.NewGuid(), "Tea", Guid.NewGuid());
        var repository = new FakeItemRepository(matchingItem, otherItem);
        var useCase = new GetLocationItemsUseCase(repository);

        var result = await useCase.ExecuteAsync(locationId, CancellationToken.None);

        var item = Assert.Single(result);
        Assert.Equal(matchingItem.Id, item.Id);
        Assert.Equal("Coffee", item.Name);
    }

    private sealed class FakeItemRepository(params Item[] seededItems) : IItemRepository
    {
        public List<Item> StoredItems { get; } = seededItems.ToList();

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
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid itemId, CancellationToken cancellationToken)
        {
            StoredItems.RemoveAll(item => item.Id == itemId);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
