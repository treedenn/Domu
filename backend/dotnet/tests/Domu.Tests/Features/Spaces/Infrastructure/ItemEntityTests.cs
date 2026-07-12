using Domu.Api.Features.Spaces.Domain.Items;
using Domu.Api.Features.Spaces.Infrastructure.Items;

namespace Domu.Tests.Features.Spaces.Infrastructure;

public sealed class ItemEntityTests
{
    [Fact]
    public void FromDomain_AndToDomain_RoundTripsAggregate()
    {
        var item = new Item(Guid.NewGuid(), "Milk", Guid.NewGuid());
        item.ChangeCategory("Dairy");
        item.ChangeBarcode("5901234123457");

        var firstEntry = new ItemEntry(Guid.NewGuid(), item.Id);
        firstEntry.SetDates(
            new DateTimeOffset(2026, 4, 19, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 4, 25, 0, 0, 0, TimeSpan.Zero));
        firstEntry.SetQuantities(2, 1);
        firstEntry.SetUnit(ItemUnit.Liter);
        firstEntry.SetContainerType(ItemContainerType.Carton);
        firstEntry.ChangeState(ConsumableState.Opened);
        item.AddEntry(firstEntry);

        var entity = ItemEntity.FromDomain(item);
        var roundTrippedItem = entity.ToDomain();

        Assert.Equal(item.Id, roundTrippedItem.Id);
        Assert.Equal(item.SpaceId, roundTrippedItem.SpaceId);
        Assert.Equal(item.Name, roundTrippedItem.Name);
        Assert.Equal(item.Category, roundTrippedItem.Category);
        Assert.Equal(item.Barcode, roundTrippedItem.Barcode);
        Assert.Equal(item.TotalQuantity, roundTrippedItem.TotalQuantity);
        var roundTrippedEntry = Assert.Single(roundTrippedItem.Entries);
        Assert.Equal(ItemUnit.Liter, roundTrippedEntry.Unit);
        Assert.Equal(ItemContainerType.Carton, roundTrippedEntry.ContainerType);
    }

    [Fact]
    public void UpdateFromDomain_SynchronizesAddedUpdatedAndRemovedEntries()
    {
        var item = new Item(Guid.NewGuid(), "Beans", Guid.NewGuid());
        var existingEntry = new ItemEntry(Guid.NewGuid(), item.Id);
        existingEntry.SetQuantities(1, 1);
        item.AddEntry(existingEntry);

        var entity = ItemEntity.FromDomain(item);

        item.Rename("Kidney Beans");
        existingEntry.SetQuantities(4, 3.5m);

        var newEntry = new ItemEntry(Guid.NewGuid(), item.Id);
        newEntry.SetQuantities(2, 2);
        item.AddEntry(newEntry);

        entity.UpdateFromDomain(item);

        Assert.Equal("Kidney Beans", entity.Name);
        Assert.Equal(2, entity.Entries.Count);
        Assert.Contains(entity.Entries, entry => entry.Id == existingEntry.Id && entry.CurrentQuantity == 3.5m);
        Assert.Contains(entity.Entries, entry => entry.Id == newEntry.Id && entry.CurrentQuantity == 2);
    }
}