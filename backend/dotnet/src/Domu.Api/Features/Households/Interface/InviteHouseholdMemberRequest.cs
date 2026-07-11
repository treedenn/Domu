using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Interface;

public sealed record InviteHouseholdMemberRequest(
    string Email,
    string DisplayName,
    [DisallowUnspecifiedHouseholdMemberRole]
    HouseholdMemberRole Role = HouseholdMemberRole.Member);
