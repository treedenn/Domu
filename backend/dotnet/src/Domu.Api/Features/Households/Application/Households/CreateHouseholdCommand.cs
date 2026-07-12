using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Households.Application.Households;

public sealed record CreateHouseholdCommand(DomuActor Actor, string Name, string OwnerDisplayName);