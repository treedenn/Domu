using Domu.Api.Features.Households.Application.Households.Contracts;

namespace Domu.Api.Features.Households.Application.Households;

public interface IGetHouseholdUseCase
{
    Task<HouseholdView> ExecuteAsync(GetHouseholdQuery query, CancellationToken cancellationToken);
}
