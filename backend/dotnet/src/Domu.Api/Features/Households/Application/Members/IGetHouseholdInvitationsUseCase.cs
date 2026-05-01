using Domu.Api.Features.Households.Application.Members.Contracts;

namespace Domu.Api.Features.Households.Application.Members;

public interface IGetHouseholdInvitationsUseCase
{
    Task<IReadOnlyList<HouseholdInvitationView>> ExecuteAsync(
        GetHouseholdInvitationsQuery query,
        CancellationToken cancellationToken);
}
