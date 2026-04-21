using Domu.Api.Features.Households.Application.Households.Contracts;
using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Domain.Households;

namespace Domu.Api.Features.Households.Application.Households;

public sealed class CreateHouseholdUseCase(IHouseholdRepository householdRepository) : ICreateHouseholdUseCase
{
    public async Task<HouseholdView> ExecuteAsync(CreateHouseholdCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var household = new Household(Guid.CreateVersion7(), command.OwnerId, command.Name);

        await householdRepository.AddAsync(household, cancellationToken);
        await householdRepository.SaveChangesAsync(cancellationToken);

        return HouseholdView.FromDomain(household);
    }
}
