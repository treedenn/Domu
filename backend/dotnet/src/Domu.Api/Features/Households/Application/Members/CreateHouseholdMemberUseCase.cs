using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members.Contracts;
using Domu.Api.Features.Households.Application.Members.Ports;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Application.Members;

public sealed class CreateHouseholdMemberUseCase(
    IHouseholdRepository householdRepository,
    IHouseholdMembershipRepository membershipRepository)
    : ICreateHouseholdMemberUseCase
{
    public async Task<HouseholdMemberView> ExecuteAsync(
        CreateHouseholdMemberCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = await householdRepository.GetByIdAsync(command.HouseholdId, cancellationToken)
                        ?? throw new KeyNotFoundException($"Household '{command.HouseholdId}' was not found.");

        if (household.OwnerId != command.CreatedByUserId)
            throw new KeyNotFoundException($"Household '{command.HouseholdId}' was not found.");
        if (command.Role is HouseholdMemberRole.Owner or HouseholdMemberRole.Unspecified)
            throw new ArgumentException("Accountless members must have the admin or member role.", nameof(command));

        var member = new HouseholdMember(
            Guid.CreateVersion7(),
            command.HouseholdId,
            null,
            command.DisplayName,
            command.Role,
            DateTimeOffset.UtcNow);

        await membershipRepository.AddMemberAsync(member, cancellationToken);
        await membershipRepository.SaveChangesAsync(cancellationToken);

        return HouseholdMemberView.FromDomain(member);
    }
}
