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
    public void SetQuantities_WithNegativeInitialValue_Throws()
    {
        var entry = new ItemEntry(Guid.NewGuid(), Guid.NewGuid());

        var action = () => entry.SetQuantities(-1, 0);

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("initial quantity must be >= 0", exception.Message);
    }

    [Fact]
    public void SetQuantities_UpdatesQuantities()
    {
        var entry = new ItemEntry(Guid.NewGuid(), Guid.NewGuid());

        entry.SetQuantities(2.5m, 1.25m);

        Assert.Equal(2.5m, entry.InitialQuantity);
        Assert.Equal(1.25m, entry.CurrentQuantity);
    }

    [Fact]
    public void SetQuantities_WithCurrentGreaterThanInitial_Throws()
    {
        var entry = new ItemEntry(Guid.NewGuid(), Guid.NewGuid());

        var action = () => entry.SetQuantities(1, 2);

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("current quantity cannot be greater than initial quantity", exception.Message);
    }

    [Fact]
    public void ChangeState_UpdatesState()
    {
        var entry = new ItemEntry(Guid.NewGuid(), Guid.NewGuid());

        entry.ChangeState(ConsumableState.Opened);

        Assert.Equal(ConsumableState.Opened, entry.State);
    }

    [Fact]
    public void SetUnit_UpdatesUnit()
    {
        var entry = new ItemEntry(Guid.NewGuid(), Guid.NewGuid());

        entry.SetUnit(ItemUnit.Liter);

        Assert.Equal(ItemUnit.Liter, entry.Unit);
    }

    [Fact]
    public void SetContainerType_UpdatesContainerType()
    {
        var entry = new ItemEntry(Guid.NewGuid(), Guid.NewGuid());

        entry.SetContainerType(ItemContainerType.Carton);

        Assert.Equal(ItemContainerType.Carton, entry.ContainerType);
    }
}
