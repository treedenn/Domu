using Domu.Api.Features.Households.Application.Members.Ports;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Tests.Features.Households.Application;

internal sealed class FakeHouseholdInvitationSender : IHouseholdInvitationSender
{
    public List<HouseholdInvitation> SentInvitations { get; } = [];

    public Task SendAsync(HouseholdInvitation invitation, CancellationToken cancellationToken)
    {
        SentInvitations.Add(invitation);
        return Task.CompletedTask;
    }
}
