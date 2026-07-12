using Domu.Api.Features.Spaces.Domain.Spaces;
using Domu.Api.Features.Spaces.Infrastructure.Spaces;

namespace Domu.Tests.Features.Spaces.Infrastructure;

public sealed class SpaceEntityTests
{
    [Fact]
    public void FromDomain_AndToDomain_RoundTripsSpace()
    {
        var space = new Space(Guid.NewGuid(), "Pantry", Guid.NewGuid());
        var parentId = Guid.NewGuid();
        space.Describe("Food storage");
        space.MoveTo(parentId);

        var entity = SpaceEntity.FromDomain(space);
        var roundTrippedSpace = entity.ToDomain();

        Assert.Equal(space.Id, roundTrippedSpace.Id);
        Assert.Equal(space.HouseholdId, roundTrippedSpace.HouseholdId);
        Assert.Equal(space.Name, roundTrippedSpace.Name);
        Assert.Equal(space.Description, roundTrippedSpace.Description);
        Assert.Equal(space.ParentId, roundTrippedSpace.ParentId);
    }

    [Fact]
    public void UpdateFromDomain_UpdatesScalarFields()
    {
        var space = new Space(Guid.NewGuid(), "Pantry", Guid.NewGuid());
        var entity = SpaceEntity.FromDomain(space);
        var parentId = Guid.NewGuid();

        space.Rename("Kitchen Pantry");
        space.Describe("Updated");
        space.MoveTo(parentId);

        entity.UpdateFromDomain(space);

        Assert.Equal("Kitchen Pantry", entity.Name);
        Assert.Equal("Updated", entity.Description);
        Assert.Equal(parentId, entity.ParentId);
    }
}