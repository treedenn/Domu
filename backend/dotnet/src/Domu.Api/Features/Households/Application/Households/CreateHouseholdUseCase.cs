using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Households.Application.Households.Contracts;
using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members.Ports;
using Domu.Api.Features.Households.Domain.Households;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Application.Households;

public sealed class CreateHouseholdUseCase(
    IHouseholdRepository householdRepository,
    IHouseholdMembershipRepository membershipRepository,
    IUserEventRecorder? userEventRecorder = null)
    : ICreateHouseholdUseCase
{
    private readonly IUserEventRecorder _userEventRecorder = userEventRecorder ?? NoOpUserEventRecorder.Instance;

    public async Task<HouseholdView> ExecuteAsync(CreateHouseholdCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = new Household(Guid.CreateVersion7(), command.OwnerId, command.Name);
        var ownerMember = new HouseholdMember(
            Guid.CreateVersion7(),
            household.Id,
            command.OwnerId,
            HouseholdMemberRole.Owner,
            DateTimeOffset.UtcNow);

        await householdRepository.AddAsync(household, cancellationToken);
        await membershipRepository.AddMemberAsync(ownerMember, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.OwnerId,
            UserEventActions.HouseholdCreated,
            UserEventTargetTypes.Household,
            household.Id,
            household.Id,
            EventMetadata.From(("name", household.Name)),
            cancellationToken);
        await householdRepository.SaveChangesAsync(cancellationToken);

        return HouseholdView.FromDomain(household);
    }
}
