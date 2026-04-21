using Domu.Api.Features.Households.Application.Households.Contracts;

namespace Domu.Api.Features.Households.Application.Households;

public interface ICreateHouseholdUseCase
{
    Task<HouseholdView> ExecuteAsync(CreateHouseholdCommand command, CancellationToken cancellationToken);
}
