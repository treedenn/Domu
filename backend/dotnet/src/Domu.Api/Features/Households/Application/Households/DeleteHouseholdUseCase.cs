using Domu.Api.Features.Activities.Application;
using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members.Ports;

namespace Domu.Api.Features.Households.Application.Households;

public sealed class DeleteHouseholdUseCase(
    IHouseholdRepository householdRepository,
    IHouseholdMembershipRepository membershipRepository,
    IHouseholdActivityRecorder? householdActivityRecorder = null)
{
    private readonly IHouseholdActivityRecorder _householdActivityRecorder =
        householdActivityRecorder ?? NoOpHouseholdActivityRecorder.Instance;

    public async Task ExecuteAsync(DeleteHouseholdCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = await householdRepository.GetByIdAsync(command.HouseholdId, cancellationToken)
                        ?? throw new KeyNotFoundException($"Household '{command.HouseholdId}' was not found.");

        if (!await membershipRepository.IsOwnerAsync(household, command.Actor.ActorId, cancellationToken))
            throw new KeyNotFoundException($"Household '{command.HouseholdId}' was not found.");

        await householdRepository.DeleteAsync(command.HouseholdId, cancellationToken);
        await _householdActivityRecorder.RecordAsync(
            command.Actor,
            HouseholdActivityActions.HouseholdDeleted,
            HouseholdActivityTargetTypes.Household,
            command.HouseholdId,
            command.HouseholdId,
            ActivityMetadata.Empty(),
            cancellationToken);
        await householdRepository.SaveChangesAsync(cancellationToken);
    }
}
