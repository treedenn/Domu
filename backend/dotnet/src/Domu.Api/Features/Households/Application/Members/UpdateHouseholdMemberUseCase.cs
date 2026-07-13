using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members.Contracts;
using Domu.Api.Features.Households.Application.Members.Ports;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Application.Members;

public sealed class UpdateHouseholdMemberUseCase(
    IHouseholdRepository householdRepository,
    IHouseholdMembershipRepository membershipRepository)
{
    public async Task<HouseholdMemberView> ExecuteAsync(
        UpdateHouseholdMemberCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = await householdRepository.GetByIdAsync(command.HouseholdId, cancellationToken)
                        ?? throw new KeyNotFoundException($"Household '{command.HouseholdId}' was not found.");

        if (!await membershipRepository.IsOwnerAsync(household, command.Actor.ActorId, cancellationToken))
            throw new KeyNotFoundException($"Household '{command.HouseholdId}' was not found.");
        if (command.Role is HouseholdMemberRole.Owner or HouseholdMemberRole.Unspecified)
            throw new ArgumentException("Household members must have the admin or member role.", nameof(command.Role));

        var member = await membershipRepository.GetMemberByIdAsync(
                         command.HouseholdId,
                         command.MemberId,
                         cancellationToken)
                     ?? throw new KeyNotFoundException($"Household member '{command.MemberId}' was not found.");

        if (member.Role == HouseholdMemberRole.Owner)
            throw new ArgumentException("The household owner member cannot be updated through this endpoint.",
                nameof(command.Role));

        member.Rename(command.DisplayName);
        member.ChangeRole(command.Role);
        member.SetArchived(command.Archived);

        await membershipRepository.UpdateMemberAsync(member, cancellationToken);
        await membershipRepository.SaveChangesAsync(cancellationToken);

        return HouseholdMemberView.FromDomain(member);
    }
}