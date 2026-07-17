using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Spaces.Application.Items;
using Domu.Api.Features.Spaces.Application.Items.Contracts;
using Domu.Api.Features.Spaces.Application.Items.Ports;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Tests.Features.Spaces.Application;

public sealed class ReplaceItemEntriesUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ReplacesItemEntries()
    {
        var item = new Item(Guid.NewGuid(), "Milk", Guid.NewGuid());
        item.AddEntry(new ItemEntry(Guid.NewGuid(), item.Id));
        var repository = new FakeItemRepository(item);
        var useCase = new ReplaceItemEntriesUseCase(repository, new FakeSpaceAccessService());

        var result = await useCase.ExecuteAsync(
            new ReplaceItemEntriesCommand(new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel),
                Guid.NewGuid(),
                item.SpaceId,
                item.Id,
                [
                    new ItemEntryDraft(null, 3, 1.5m, ItemUnit.Liter,
                        ConsumableState.Unopened, null, null)
                ]),
            CancellationToken.None);

        var entry = Assert.Single(result.Entries);
        Assert.Equal(3, entry.OriginalQuantity);
        Assert.Equal(1.5m, entry.CurrentQuantity);
        Assert.Equal(ItemUnit.Liter, entry.Unit);
        Assert.Equal(ConsumableState.Unopened, entry.State);
        Assert.Equal(1, repository.SaveChangesCalls);
    }

    [Fact]
    public async Task ExecuteAsync_NormalizesEntryDatesToUtc()
    {
        var item = new Item(Guid.NewGuid(), "Milk", Guid.NewGuid());
        var repository = new FakeItemRepository(item);
        var useCase = new ReplaceItemEntriesUseCase(repository, new FakeSpaceAccessService());
        var acquisitionDate = new DateTimeOffset(2026, 7, 17, 9, 0, 0, TimeSpan.FromHours(2));
        var expirationDate = new DateTimeOffset(2026, 7, 24, 9, 0, 0, TimeSpan.FromHours(2));

        var result = await useCase.ExecuteAsync(
            new ReplaceItemEntriesCommand(new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel),
                Guid.NewGuid(),
                item.SpaceId,
                item.Id,
                [new ItemEntryDraft(null, 1, 1, ItemUnit.Piece, ConsumableState.Unopened, acquisitionDate,
                    expirationDate)]),
            CancellationToken.None);

        var entry = Assert.Single(result.Entries);
        Assert.Equal(TimeSpan.Zero, entry.AcquisitionDate!.Value.Offset);
        Assert.Equal(acquisitionDate.UtcDateTime, entry.AcquisitionDate.Value.UtcDateTime);
        Assert.Equal(TimeSpan.Zero, entry.ExpirationDate!.Value.Offset);
        Assert.Equal(expirationDate.UtcDateTime, entry.ExpirationDate.Value.UtcDateTime);
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
