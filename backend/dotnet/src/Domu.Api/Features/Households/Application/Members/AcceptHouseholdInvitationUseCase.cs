using Domu.Api.Features.Auth.Application;
using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Households.Application.Members.Contracts;
using Domu.Api.Features.Households.Application.Members.Ports;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Application.Members;

public sealed class AcceptHouseholdInvitationUseCase(
    IHouseholdMembershipRepository membershipRepository,
    IHouseholdEventRecorder? userEventRecorder = null)
{
    private readonly IHouseholdEventRecorder _userEventRecorder = userEventRecorder ?? NoOpHouseholdEventRecorder.Instance;

    public async Task<HouseholdMemberView> ExecuteAsync(
        AcceptHouseholdInvitationCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentException.ThrowIfNullOrWhiteSpace(command.Token);

        var invitation = await membershipRepository.GetInvitationByTokenAsync(command.Token, cancellationToken)
                         ?? throw new KeyNotFoundException("Household invitation was not found.");

        var now = DateTimeOffset.UtcNow;
        invitation.Accept(now);

        var member = await membershipRepository.GetMemberAsync(invitation.HouseholdId, command.Actor.ActorId, cancellationToken);
        if (member is null)
        {
            member = new HouseholdMember(
                Guid.CreateVersion7(),
                invitation.HouseholdId,
                command.Actor.ActorId,
                invitation.DisplayName,
                invitation.Role,
                now);

            await membershipRepository.AddMemberAsync(member, cancellationToken);
        }

        await membershipRepository.UpdateInvitationAsync(invitation, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.Actor.ActorId,
            HouseholdEventActions.HouseholdInvitationAccepted,
            HouseholdEventTargetTypes.HouseholdInvitation,
            invitation.Id,
            invitation.HouseholdId,
            EventMetadata.From(("memberId", member.Id), ("role", member.Role.ToString())),
            cancellationToken);
        await membershipRepository.SaveChangesAsync(cancellationToken);

        return HouseholdMemberView.FromDomain(member);
    }
}
