using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members.Ports;

namespace Domu.Api.Features.Households.Application.Households;

public sealed class DeleteHouseholdUseCase(
    IHouseholdRepository householdRepository,
    IHouseholdMembershipRepository membershipRepository,
    IHouseholdEventRecorder? userEventRecorder = null)
{
    private readonly IHouseholdEventRecorder _userEventRecorder = userEventRecorder ?? NoOpHouseholdEventRecorder.Instance;

    public async Task ExecuteAsync(DeleteHouseholdCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = await householdRepository.GetByIdAsync(command.HouseholdId, cancellationToken)
                        ?? throw new KeyNotFoundException($"Household '{command.HouseholdId}' was not found.");

        if (!await membershipRepository.IsOwnerAsync(household, command.Actor.ActorId, cancellationToken))
            throw new KeyNotFoundException($"Household '{command.HouseholdId}' was not found.");

        await householdRepository.DeleteAsync(command.HouseholdId, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.Actor.ActorId,
            HouseholdEventActions.HouseholdDeleted,
            HouseholdEventTargetTypes.Household,
            command.HouseholdId,
            command.HouseholdId,
            EventMetadata.Empty(),
            cancellationToken);
        await householdRepository.SaveChangesAsync(cancellationToken);
    }
}
