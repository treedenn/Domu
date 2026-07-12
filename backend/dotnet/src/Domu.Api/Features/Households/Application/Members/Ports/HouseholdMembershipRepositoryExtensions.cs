using Domu.Api.Features.Households.Domain.Households;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Application.Members.Ports;

public static class HouseholdMembershipRepositoryExtensions
{
    public static async Task<bool> IsOwnerAsync(
        this IHouseholdMembershipRepository repository,
        Household household,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var member = await repository.GetMemberAsync(household.Id, userId, cancellationToken);
        return member is not null
               && !member.Archived
               && member.Role == HouseholdMemberRole.Owner;
    }
}
