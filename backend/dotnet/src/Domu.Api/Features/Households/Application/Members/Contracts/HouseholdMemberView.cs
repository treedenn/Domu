using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Application.Members.Contracts;

public sealed record HouseholdMemberView(
    Guid Id,
    Guid HouseholdId,
    Guid? UserId,
    string DisplayName,
    HouseholdMemberRole Role,
    DateTimeOffset JoinedAt,
    bool Archived)
{
    public static HouseholdMemberView FromDomain(HouseholdMember member)
    {
        return new HouseholdMemberView(
            member.Id,
            member.HouseholdId,
            member.UserId,
            member.DisplayName,
            member.Role,
            member.JoinedAt,
            member.Archived);
    }
}
