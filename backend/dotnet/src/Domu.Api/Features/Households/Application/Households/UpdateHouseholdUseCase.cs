using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Households.Application.Households.Contracts;
using Domu.Api.Features.Households.Application.Households.Ports;

namespace Domu.Api.Features.Households.Application.Households;

public sealed class UpdateHouseholdUseCase(
    IHouseholdRepository householdRepository,
    IUserEventRecorder? userEventRecorder = null)
{
    private readonly IUserEventRecorder _userEventRecorder = userEventRecorder ?? NoOpUserEventRecorder.Instance;

    public async Task<HouseholdView> ExecuteAsync(UpdateHouseholdCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = await householdRepository.GetByIdAsync(command.HouseholdId, cancellationToken)
                        ?? throw new KeyNotFoundException($"Household '{command.HouseholdId}' was not found.");

        if (household.OwnerId != command.Actor.ActorId)
            throw new KeyNotFoundException($"Household '{command.HouseholdId}' was not found.");

        household.Rename(command.Name);

        await householdRepository.UpdateAsync(household, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.Actor.ActorId,
            UserEventActions.HouseholdUpdated,
            UserEventTargetTypes.Household,
            household.Id,
            household.Id,
            EventMetadata.From(("name", household.Name)),
            cancellationToken);
        await householdRepository.SaveChangesAsync(cancellationToken);

        return HouseholdView.FromDomain(household);
    }
}
