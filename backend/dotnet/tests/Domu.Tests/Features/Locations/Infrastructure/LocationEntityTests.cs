using Domu.Api.Features.Locations.Domain.Locations;
using Domu.Api.Features.Locations.Infrastructure.Locations;

namespace Domu.Tests.Features.Locations.Infrastructure;

public sealed class LocationEntityTests
{
    [Fact]
    public void FromDomain_AndToDomain_RoundTripsLocation()
    {
        var location = new Location(Guid.NewGuid(), "Pantry", Guid.NewGuid());
        var parentId = Guid.NewGuid();
        location.Describe("Food storage");
        location.MoveTo(parentId);

        var entity = LocationEntity.FromDomain(location);
        var roundTrippedLocation = entity.ToDomain();

        Assert.Equal(location.Id, roundTrippedLocation.Id);
        Assert.Equal(location.OwnerId, roundTrippedLocation.OwnerId);
        Assert.Equal(location.Name, roundTrippedLocation.Name);
        Assert.Equal(location.Description, roundTrippedLocation.Description);
        Assert.Equal(location.ParentId, roundTrippedLocation.ParentId);
    }

    [Fact]
    public void UpdateFromDomain_UpdatesScalarFields()
    {
        var location = new Location(Guid.NewGuid(), "Pantry", Guid.NewGuid());
        var entity = LocationEntity.FromDomain(location);
        var parentId = Guid.NewGuid();

        location.Rename("Kitchen Pantry");
        location.Describe("Updated");
        location.MoveTo(parentId);

        entity.UpdateFromDomain(location);

        Assert.Equal("Kitchen Pantry", entity.Name);
        Assert.Equal("Updated", entity.Description);
        Assert.Equal(parentId, entity.ParentId);
    }
}
