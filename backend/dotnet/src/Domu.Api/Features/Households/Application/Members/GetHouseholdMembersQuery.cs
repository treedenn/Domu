using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Households.Application.Members;

public sealed record GetHouseholdMembersQuery(Guid HouseholdId, DomuActor Actor);
