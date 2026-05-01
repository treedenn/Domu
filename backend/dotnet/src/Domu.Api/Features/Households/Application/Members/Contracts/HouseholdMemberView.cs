using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Application.Members.Contracts;

public sealed record HouseholdMemberView(
    Guid Id,
    Guid HouseholdId,
    Guid UserId,
    HouseholdMemberRole Role,
    DateTimeOffset JoinedAt)
{
    public static HouseholdMemberView FromDomain(HouseholdMember member)
    {
        return new HouseholdMemberView(
            member.Id,
            member.HouseholdId,
            member.UserId,
            member.Role,
            member.JoinedAt);
    }
}
