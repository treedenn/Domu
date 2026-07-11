using System.Security.Cryptography;
using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members.Contracts;
using Domu.Api.Features.Households.Application.Members.Ports;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Application.Members;

public sealed class InviteHouseholdMemberUseCase(
    IHouseholdRepository householdRepository,
    IHouseholdMembershipRepository membershipRepository,
    IHouseholdInvitationSender invitationSender,
    IUserEventRecorder? userEventRecorder = null)
{
    private readonly IUserEventRecorder _userEventRecorder = userEventRecorder ?? NoOpUserEventRecorder.Instance;

    public async Task<HouseholdInvitationView> ExecuteAsync(
        InviteHouseholdMemberCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = await householdRepository.GetByIdAsync(command.HouseholdId, cancellationToken)
                        ?? throw new KeyNotFoundException($"Household '{command.HouseholdId}' was not found.");

        if (household.OwnerId != command.InvitedByUserId)
            throw new KeyNotFoundException($"Household '{command.HouseholdId}' was not found.");

        var inviterMember = await membershipRepository.GetMemberAsync(
            command.HouseholdId,
            command.InvitedByUserId,
            cancellationToken)
            ?? throw new InvalidOperationException(
                $"Household '{command.HouseholdId}' is owned by user '{command.InvitedByUserId}' but has no linked household member.");

        var email = HouseholdInvitation.NormalizeEmail(command.Email);
        var existingInvitation = await membershipRepository.GetPendingInvitationByEmailAsync(
            command.HouseholdId,
            email,
            cancellationToken);

        if (existingInvitation is not null)
        {
            await invitationSender.SendAsync(existingInvitation, cancellationToken);
            return HouseholdInvitationView.FromDomain(existingInvitation);
        }

        var now = DateTimeOffset.UtcNow;
        var invitation = new HouseholdInvitation(
            Guid.CreateVersion7(),
            command.HouseholdId,
            email,
            command.DisplayName,
            inviterMember.Id,
            command.Role,
            GenerateToken(),
            now,
            now.Add(HouseholdInvitation.DefaultLifetime));

        await membershipRepository.AddInvitationAsync(invitation, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.InvitedByUserId,
            UserEventActions.HouseholdMemberInvited,
            UserEventTargetTypes.HouseholdInvitation,
            invitation.Id,
            command.HouseholdId,
            EventMetadata.From(("email", invitation.Email), ("role", invitation.Role.ToString())),
            cancellationToken);
        await membershipRepository.SaveChangesAsync(cancellationToken);
        await invitationSender.SendAsync(invitation, cancellationToken);

        return HouseholdInvitationView.FromDomain(invitation);
    }

    private static string GenerateToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
