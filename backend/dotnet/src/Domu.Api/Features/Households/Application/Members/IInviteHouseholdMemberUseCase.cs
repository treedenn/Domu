using Domu.Api.Features.Households.Application.Members.Contracts;

namespace Domu.Api.Features.Households.Application.Members;

public interface IInviteHouseholdMemberUseCase
{
    Task<HouseholdInvitationView> ExecuteAsync(InviteHouseholdMemberCommand command, CancellationToken cancellationToken);
}
