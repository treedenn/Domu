using Domu.Api.Features.Spaces.Domain.Items;
using Domu.Api.Features.Spaces.Domain.Spaces;

namespace Domu.Tests.Features.Spaces.Domain;

public sealed class SpaceUnitTest
{
    [Fact]
    public void Rename_UpdatesName()
    {
        var space = new Space(Guid.NewGuid(), "Pantry", Guid.NewGuid());

        space.Rename("Kitchen");

        Assert.Equal("Kitchen", space.Name);
    }

    [Fact]
    public void AddChild_SetsParentIdAndAddsChild()
    {
        var householdId = Guid.NewGuid();
        var parent = new Space(Guid.NewGuid(), "House", householdId);
        var child = new Space(Guid.NewGuid(), "Kitchen", householdId);

        var added = parent.AddChild(child);

        Assert.True(added);
        Assert.Equal(parent.Id, child.ParentId);
        Assert.Contains(child, parent.Children);
    }

    [Fact]
    public void AddChild_FromDifferentHousehold_Throws()
    {
        var parent = new Space(Guid.NewGuid(), "House", Guid.NewGuid());
        var child = new Space(Guid.NewGuid(), "Kitchen", Guid.NewGuid());

        void Action()
        {
            parent.AddChild(child);
        }

        var exception = Assert.Throws<ArgumentException>(Action);
        Assert.Contains("same household", exception.Message);
    }

    [Fact]
    public void AddItem_WithDifferentSpace_Throws()
    {
        var space = new Space(Guid.NewGuid(), "Pantry", Guid.NewGuid());
        var item = new Item(Guid.NewGuid(), "Pasta", Guid.NewGuid());

        void Action()
        {
            space.AddItem(item);
        }

        var exception = Assert.Throws<ArgumentException>(Action);
        Assert.Contains("Item must belong to this space.", exception.Message);
    }
}