using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Application.Members;

public sealed record CreateHouseholdMemberCommand(
    Guid HouseholdId,
    Guid CreatedByUserId,
    string DisplayName,
    HouseholdMemberRole Role);
