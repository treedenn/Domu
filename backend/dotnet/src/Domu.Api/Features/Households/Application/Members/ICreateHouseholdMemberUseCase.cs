using Domu.Api.Features.Households.Application.Members.Contracts;

namespace Domu.Api.Features.Households.Application.Members;

public interface ICreateHouseholdMemberUseCase
{
    Task<HouseholdMemberView> ExecuteAsync(CreateHouseholdMemberCommand command, CancellationToken cancellationToken);
}
