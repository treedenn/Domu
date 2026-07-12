using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Spaces.Application.Items;
using Domu.Api.Features.Spaces.Application.Items.Ports;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Tests.Features.Spaces.Application;

public sealed class GetSpaceItemsUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsItemsForSpace()
    {
        var spaceId = Guid.NewGuid();
        var matchingItem = new Item(Guid.NewGuid(), "Coffee", spaceId);
        var repository = new FakeItemRepository(matchingItem, new Item(Guid.NewGuid(), "Tea", Guid.NewGuid()));

        var useCase = new GetSpaceItemsUseCase(repository, new FakeSpaceAccessService());

        var result = await useCase.ExecuteAsync(
            new GetSpaceItemsQuery(new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel), Guid.NewGuid(), spaceId),
            CancellationToken.None);

        var item = Assert.Single(result);
        Assert.Equal(matchingItem.Id, item.Id);
    }

    private sealed class FakeItemRepository(params Item[] seededItems) : IItemRepository
    {
        public List<Item> StoredItems { get; } = seededItems.ToList();

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
            return Task.CompletedTask;
        }
    }
}