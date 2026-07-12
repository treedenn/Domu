using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Households.Application.Households;

public sealed record UpdateHouseholdCommand(Guid HouseholdId, DomuActor Actor, string Name);
