namespace Domu.Api.Features.Households.Application.Members.Contracts;

public sealed record HouseholdMembersView(
    IReadOnlyList<HouseholdMemberView> Members,
    bool CanManageMembers);
