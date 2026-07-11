using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Application.Members;

public sealed record UpdateHouseholdMemberCommand(
    Guid HouseholdId,
    Guid MemberId,
    Guid UpdatedByUserId,
    string DisplayName,
    HouseholdMemberRole Role,
    bool Archived);
