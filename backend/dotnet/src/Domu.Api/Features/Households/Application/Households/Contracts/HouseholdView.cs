using Domu.Api.Features.Households.Domain.Households;

namespace Domu.Api.Features.Households.Application.Households.Contracts;

public sealed record HouseholdView(Guid Id, Guid OwnerId, string Name)
{
    public static HouseholdView FromDomain(Household household)
    {
        ArgumentNullException.ThrowIfNull(household);

        return new HouseholdView(household.Id, household.OwnerId, household.Name);
    }
}
