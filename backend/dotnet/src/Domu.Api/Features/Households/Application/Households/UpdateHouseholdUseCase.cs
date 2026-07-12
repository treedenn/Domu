using Domu.Api.Features.Activities.Application;
using Domu.Api.Features.Households.Application.Households.Contracts;
using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members.Ports;

namespace Domu.Api.Features.Households.Application.Households;

public sealed class UpdateHouseholdUseCase(
    IHouseholdRepository householdRepository,
    IHouseholdMembershipRepository membershipRepository,
    IHouseholdActivityRecorder? householdActivityRecorder = null)
{
    private readonly IHouseholdActivityRecorder _householdActivityRecorder =
        householdActivityRecorder ?? NoOpHouseholdActivityRecorder.Instance;

    public async Task<HouseholdView> ExecuteAsync(UpdateHouseholdCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = await householdRepository.GetByIdAsync(command.HouseholdId, cancellationToken)
                        ?? throw new KeyNotFoundException($"Household '{command.HouseholdId}' was not found.");

        if (!await membershipRepository.IsOwnerAsync(household, command.Actor.ActorId, cancellationToken))
            throw new KeyNotFoundException($"Household '{command.HouseholdId}' was not found.");

        household.Rename(command.Name);

        await householdRepository.UpdateAsync(household, cancellationToken);
        await _householdActivityRecorder.RecordAsync(
            command.Actor,
            HouseholdActivityActions.HouseholdUpdated,
            HouseholdActivityTargetTypes.Household,
            household.Id,
            household.Id,
            ActivityMetadata.From(("name", household.Name)),
            cancellationToken);
        await householdRepository.SaveChangesAsync(cancellationToken);

        return HouseholdView.FromDomain(household);
    }
}
