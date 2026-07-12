using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Application.Members;

public sealed record UpdateHouseholdMemberCommand(
    DomuActor Actor,
    Guid HouseholdId,
    Guid MemberId,
    string DisplayName,
    HouseholdMemberRole Role,
    bool Archived);