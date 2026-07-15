using System.Text.Json.Serialization;
using Domu.Api.Features.Households.Application.Members.Contracts;

namespace Domu.Api.Features.Households.Interface;

public sealed record HouseholdMembersResponse(
    [property: JsonPropertyName("data")] IReadOnlyList<HouseholdMemberView> Data,
    bool CanManageMembers);
