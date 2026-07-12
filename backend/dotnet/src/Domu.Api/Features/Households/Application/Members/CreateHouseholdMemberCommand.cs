using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Application.Members;

public sealed record CreateHouseholdMemberCommand(
    DomuActor Actor,
    Guid HouseholdId,
    string DisplayName,
    HouseholdMemberRole Role);
