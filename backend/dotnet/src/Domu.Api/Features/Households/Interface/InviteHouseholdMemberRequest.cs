using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Interface;

public sealed record InviteHouseholdMemberRequest(string Email, HouseholdMemberRole Role = HouseholdMemberRole.Member);
