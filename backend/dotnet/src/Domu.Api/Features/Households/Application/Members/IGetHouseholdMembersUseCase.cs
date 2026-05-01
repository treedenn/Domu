using Domu.Api.Features.Households.Application.Members.Contracts;

namespace Domu.Api.Features.Households.Application.Members;

public interface IGetHouseholdMembersUseCase
{
    Task<IReadOnlyList<HouseholdMemberView>> ExecuteAsync(GetHouseholdMembersQuery query, CancellationToken cancellationToken);
}
