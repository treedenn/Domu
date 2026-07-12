using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Households.Application.Households;

public sealed record GetHouseholdQuery(Guid HouseholdId, DomuActor Actor);