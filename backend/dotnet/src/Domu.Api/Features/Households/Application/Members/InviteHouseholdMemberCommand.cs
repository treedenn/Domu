using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Application.Members;

public sealed record InviteHouseholdMemberCommand(
    DomuActor Actor,
    Guid HouseholdId,
    string Email,
    string DisplayName,
    HouseholdMemberRole Role);
