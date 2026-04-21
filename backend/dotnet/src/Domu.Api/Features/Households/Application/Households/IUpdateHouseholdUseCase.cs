using Domu.Api.Features.Households.Application.Households.Contracts;

namespace Domu.Api.Features.Households.Application.Households;

public interface IUpdateHouseholdUseCase
{
    Task<HouseholdView> ExecuteAsync(UpdateHouseholdCommand command, CancellationToken cancellationToken);
}
