using Domu.Api.Features.Households.Application.Members.Contracts;

namespace Domu.Api.Features.Households.Application.Members;

public interface IAcceptHouseholdInvitationUseCase
{
    Task<HouseholdMemberView> ExecuteAsync(AcceptHouseholdInvitationCommand command, CancellationToken cancellationToken);
}
