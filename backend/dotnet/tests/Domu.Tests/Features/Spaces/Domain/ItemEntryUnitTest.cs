using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Tests.Features.Spaces.Domain;

public sealed class ItemEntryUnitTest
{
    [Fact]
    public void SetBatch_AllowsCountOnlyEntry()
    {
        var entry = new ItemEntry(Guid.NewGuid(), Guid.NewGuid());

        entry.SetBatch(2, null, null);

        Assert.Equal(2, entry.Count);
        Assert.Null(entry.OriginalAmountPerUnit);
        Assert.Null(entry.CurrentAmountPerUnit);
        Assert.Equal(ItemUnit.Unspecified, entry.Unit);
        Assert.Equal(ConsumableState.Unspecified, entry.State);
    }

    [Fact]
    public void SetBatch_RequiresBothAmounts()
    {
        var entry = new ItemEntry(Guid.NewGuid(), Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => entry.SetBatch(1, 1, null));
    }

    [Fact]
    public void SetBatch_RejectsNonPositiveCount()
    {
        var entry = new ItemEntry(Guid.NewGuid(), Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => entry.SetBatch(0, null, null));
    }
}
