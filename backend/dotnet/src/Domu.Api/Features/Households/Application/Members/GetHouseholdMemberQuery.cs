using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Households.Application.Members;

public sealed record GetHouseholdMemberQuery(DomuActor Actor, Guid HouseholdId, Guid MemberId);