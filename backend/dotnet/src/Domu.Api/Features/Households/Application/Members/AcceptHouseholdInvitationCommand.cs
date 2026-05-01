namespace Domu.Api.Features.Households.Application.Members;

public sealed record AcceptHouseholdInvitationCommand(string Token, Guid UserId);
