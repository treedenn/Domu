using Domu.Api.Features.Locations.Domain.Invitations;

namespace Domu.Tests.Features.Locations.Domain;

public sealed class LocationInvitationUnitTest
{
    [Fact]
    public void Accept_WhenPending_UpdatesStatus()
    {
        var invitation = CreateInvitation();

        invitation.Accept();

        Assert.Equal(LocationInvitationStatus.Accepted, invitation.Status);
    }

    [Fact]
    public void Revoke_AfterAccept_Throws()
    {
        var invitation = CreateInvitation();
        invitation.Accept();

        var action = () => invitation.Revoke();

        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Contains("must be pending", exception.Message);
    }

    [Fact]
    public void Expire_WhenPending_UpdatesStatus()
    {
        var invitation = CreateInvitation();

        invitation.Expire();

        Assert.Equal(LocationInvitationStatus.Expired, invitation.Status);
    }

    private static LocationInvitation CreateInvitation()
    {
        return new LocationInvitation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "test@example.com",
            "token",
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow);
    }
}
