using Domu.Api.Features.Households.Application.Households.Contracts;

namespace Domu.Api.Features.Households.Application.Households;

public interface IGetHouseholdsUseCase
{
    Task<IReadOnlyList<HouseholdView>> ExecuteAsync(GetHouseholdsQuery query, CancellationToken cancellationToken);
}
