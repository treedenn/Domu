using Domu.Api.Features.Households.Application.Households.Ports;

namespace Domu.Api.Features.Households.Application.Households;

public sealed class DeleteHouseholdUseCase(IHouseholdRepository householdRepository) : IDeleteHouseholdUseCase
{
    public async Task ExecuteAsync(DeleteHouseholdCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = await householdRepository.GetByIdAsync(command.HouseholdId, cancellationToken)
                        ?? throw new KeyNotFoundException($"Household '{command.HouseholdId}' was not found.");

        if (household.OwnerId != command.OwnerId)
            throw new KeyNotFoundException($"Household '{command.HouseholdId}' was not found.");

        await householdRepository.DeleteAsync(command.HouseholdId, cancellationToken);
        await householdRepository.SaveChangesAsync(cancellationToken);
    }
}
