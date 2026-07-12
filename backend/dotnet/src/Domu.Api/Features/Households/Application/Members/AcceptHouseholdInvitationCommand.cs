using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Households.Application.Members;

public sealed record AcceptHouseholdInvitationCommand(DomuActor Actor, string Token);