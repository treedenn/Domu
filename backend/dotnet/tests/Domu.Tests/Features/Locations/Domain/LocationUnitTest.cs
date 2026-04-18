using Domu.Api.Features.Locations.Domain.Invitations;
using Domu.Api.Features.Locations.Domain.Items;
using Domu.Api.Features.Locations.Domain.Locations;
using Domu.Api.Features.Locations.Domain.Membership;

namespace Domu.Tests.Features.Locations.Domain;

public sealed class LocationUnitTest
{
    [Fact]
    public void Rename_UpdatesName()
    {
        var location = new Location(Guid.NewGuid(), "Pantry", Guid.NewGuid());

        location.Rename("Kitchen");

        Assert.Equal("Kitchen", location.Name);
    }

    [Fact]
    public void AddChild_SetsParentIdAndAddsChild()
    {
        var parent = new Location(Guid.NewGuid(), "House", Guid.NewGuid());
        var child = new Location(Guid.NewGuid(), "Kitchen", Guid.NewGuid());

        var added = parent.AddChild(child);

        Assert.True(added);
        Assert.Equal(parent.Id, child.ParentId);
        Assert.Contains(child, parent.Children);
    }

    [Fact]
    public void AddItem_WithDifferentLocation_Throws()
    {
        var location = new Location(Guid.NewGuid(), "Pantry", Guid.NewGuid());
        var item = new Item(Guid.NewGuid(), "Pasta", Guid.NewGuid());

        void Action() => location.AddItem(item);

        var exception = Assert.Throws<ArgumentException>(Action);
        Assert.Contains("Item must belong to this location.", exception.Message);
    }

    [Fact]
    public void AddMember_OwnerAsMember_Throws()
    {
        var ownerId = Guid.NewGuid();
        var location = new Location(Guid.NewGuid(), "Pantry", ownerId);
        var member = new LocationMember(ownerId, location.Id, MembershipRole.Member);

        void Action() => location.AddMember(member);

        var exception = Assert.Throws<InvalidOperationException>(Action);
        Assert.Contains("Owner should not be added as a location member.", exception.Message);
    }

    [Fact]
    public void IsMember_ReturnsTrueForExplicitMember()
    {
        var location = new Location(Guid.NewGuid(), "Pantry", Guid.NewGuid());
        var member = new LocationMember(Guid.NewGuid(), location.Id, MembershipRole.Member);
        location.AddMember(member);

        var isMember = location.IsMember(member.UserId);

        Assert.True(isMember);
    }

    [Fact]
    public void RemoveMember_RemovesByUserId()
    {
        var location = new Location(Guid.NewGuid(), "Pantry", Guid.NewGuid());
        var member = new LocationMember(Guid.NewGuid(), location.Id, MembershipRole.Member);
        location.AddMember(member);

        var removed = location.RemoveMember(member.UserId);

        Assert.True(removed);
        Assert.DoesNotContain(location.Members, existing => existing.UserId == member.UserId);
    }
}