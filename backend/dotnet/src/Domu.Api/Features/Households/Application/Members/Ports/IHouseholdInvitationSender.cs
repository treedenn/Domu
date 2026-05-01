using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Application.Members.Ports;

public interface IHouseholdInvitationSender
{
    Task SendAsync(HouseholdInvitation invitation, CancellationToken cancellationToken);
}
