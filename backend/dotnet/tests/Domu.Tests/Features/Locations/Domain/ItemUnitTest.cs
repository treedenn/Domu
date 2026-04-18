using Domu.Api.Features.Locations.Domain.Items;

namespace Domu.Tests.Features.Locations.Domain;

public sealed class ItemUnitTest
{
    [Fact]
    public void MoveTo_UpdatesLocationId()
    {
        var item = new Item(Guid.NewGuid(), "Rice", Guid.NewGuid());
        var newLocationId = Guid.NewGuid();

        item.MoveTo(newLocationId);

        Assert.Equal(newLocationId, item.LocationId);
    }

    [Fact]
    public void ChangeCategory_ToWhitespace_Throws()
    {
        var item = new Item(Guid.NewGuid(), "Rice", Guid.NewGuid());

        var action = () => item.ChangeCategory(" ");

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("Item category cannot be empty.", exception.Message);
    }

    [Fact]
    public void RemoveEntry_RemovesByEntryId()
    {
        var item = new Item(Guid.NewGuid(), "Rice", Guid.NewGuid());
        var entry = new ItemEntry(Guid.NewGuid(), item.Id);
        item.AddEntry(entry);

        var removed = item.RemoveEntry(entry.Id);

        Assert.True(removed);
        Assert.DoesNotContain(item.Entries, existing => existing.Id == entry.Id);
    }
}