using Domu.Api.Features.Activities.Application;
using Domu.Api.Features.Households.Application.Households.Contracts;
using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members.Ports;
using Domu.Api.Features.Households.Domain.Households;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Application.Households;

public sealed class CreateHouseholdUseCase(
    IHouseholdRepository householdRepository,
    IHouseholdMembershipRepository membershipRepository,
    IHouseholdActivityRecorder? householdActivityRecorder = null)
{
    private readonly IHouseholdActivityRecorder _householdActivityRecorder =
        householdActivityRecorder ?? NoOpHouseholdActivityRecorder.Instance;

    public async Task<HouseholdView> ExecuteAsync(CreateHouseholdCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = new Household(Guid.CreateVersion7(), command.Name);
        var ownerMember = new HouseholdMember(
            Guid.CreateVersion7(),
            household.Id,
            command.Actor.ActorId,
            command.OwnerDisplayName,
            HouseholdMemberRole.Owner,
            DateTimeOffset.UtcNow);

        await householdRepository.AddAsync(household, cancellationToken);
        await membershipRepository.AddMemberAsync(ownerMember, cancellationToken);
        await _householdActivityRecorder.RecordAsync(
            command.Actor,
            HouseholdActivityActions.HouseholdCreated,
            HouseholdActivityTargetTypes.Household,
            household.Id,
            household.Id,
            ActivityMetadata.From(("name", household.Name)),
            cancellationToken);
        await householdRepository.SaveChangesAsync(cancellationToken);

        return HouseholdView.FromDomain(household);
    }
}
