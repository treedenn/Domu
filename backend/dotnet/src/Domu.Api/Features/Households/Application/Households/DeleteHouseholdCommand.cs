using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Households.Application.Households;

public sealed record DeleteHouseholdCommand(Guid HouseholdId, DomuActor Actor);