using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Households.Application.Households.Ports;

namespace Domu.Api.Features.Households.Application.Households;

public sealed class DeleteHouseholdUseCase(
    IHouseholdRepository householdRepository,
    IUserEventRecorder? userEventRecorder = null)
{
    private readonly IUserEventRecorder _userEventRecorder = userEventRecorder ?? NoOpUserEventRecorder.Instance;

    public async Task ExecuteAsync(DeleteHouseholdCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = await householdRepository.GetByIdAsync(command.HouseholdId, cancellationToken)
                        ?? throw new KeyNotFoundException($"Household '{command.HouseholdId}' was not found.");

        if (household.OwnerId != command.OwnerId)
            throw new KeyNotFoundException($"Household '{command.HouseholdId}' was not found.");

        await householdRepository.DeleteAsync(command.HouseholdId, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.OwnerId,
            UserEventActions.HouseholdDeleted,
            UserEventTargetTypes.Household,
            command.HouseholdId,
            command.HouseholdId,
            EventMetadata.Empty(),
            cancellationToken);
        await householdRepository.SaveChangesAsync(cancellationToken);
    }
}
