using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Tests.Features.Spaces.Domain;

public sealed class ItemEntryUnitTest
{
    [Fact]
    public void SetDates_WithAcquisitionAfterExpiration_Throws()
    {
        var entry = new ItemEntry(Guid.NewGuid(), Guid.NewGuid());
        var acquisition = new DateTimeOffset(2026, 4, 20, 0, 0, 0, TimeSpan.Zero);
        var expiration = new DateTimeOffset(2026, 4, 19, 0, 0, 0, TimeSpan.Zero);

        var action = () => entry.SetDates(acquisition, expiration);

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("acquisition date cannot be after expiration date", exception.Message);
    }

    [Fact]
    public void SetQuantity_WithNegativeValue_Throws()
    {
        var entry = new ItemEntry(Guid.NewGuid(), Guid.NewGuid());

        var action = () => entry.SetQuantity(-1);

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("quantity must be >= 0", exception.Message);
    }

    [Fact]
    public void ChangeState_UpdatesState()
    {
        var entry = new ItemEntry(Guid.NewGuid(), Guid.NewGuid());

        entry.ChangeState(ConsumableState.Opened);

        Assert.Equal(ConsumableState.Opened, entry.State);
    }
}