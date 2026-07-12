using Domu.Api.Features.Households.Application.Members.Ports;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Infrastructure.Members;

public sealed class LoggingHouseholdInvitationSender(ILogger<LoggingHouseholdInvitationSender> logger)
    : IHouseholdInvitationSender
{
    public Task SendAsync(HouseholdInvitation invitation, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Household invitation {InvitationId} for household {HouseholdId} queued for {Email}. Token: {Token}",
            invitation.Id,
            invitation.HouseholdId,
            invitation.Email,
            invitation.Token);

        return Task.CompletedTask;
    }
}