using Domu.Api.Features.Households.Application.Households.Contracts;
using Domu.Api.Features.Households.Application.Households.Ports;

namespace Domu.Api.Features.Households.Application.Households;

public sealed class GetHouseholdsUseCase(IHouseholdRepository householdRepository)
{
    public async Task<IReadOnlyList<HouseholdView>> ExecuteAsync(
        GetHouseholdsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var households = await householdRepository.GetAccessibleByUserIdAsync(query.OwnerId, cancellationToken);

        return households.Select(HouseholdView.FromDomain).ToArray();
    }
}
