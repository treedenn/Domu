using Domu.Api.Features.Households.Application.Households.Contracts;
using Domu.Api.Features.Households.Application.Households.Ports;

namespace Domu.Api.Features.Households.Application.Households;

public sealed class UpdateHouseholdUseCase(IHouseholdRepository householdRepository) : IUpdateHouseholdUseCase
{
    public async Task<HouseholdView> ExecuteAsync(UpdateHouseholdCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = await householdRepository.GetByIdAsync(command.HouseholdId, cancellationToken)
                        ?? throw new KeyNotFoundException($"Household '{command.HouseholdId}' was not found.");

        if (household.OwnerId != command.OwnerId)
            throw new KeyNotFoundException($"Household '{command.HouseholdId}' was not found.");

        household.Rename(command.Name);

        await householdRepository.UpdateAsync(household, cancellationToken);
        await householdRepository.SaveChangesAsync(cancellationToken);

        return HouseholdView.FromDomain(household);
    }
}
