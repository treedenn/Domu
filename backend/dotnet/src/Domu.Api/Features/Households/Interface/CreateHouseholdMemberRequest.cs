using System.ComponentModel.DataAnnotations;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Interface;

public sealed record CreateHouseholdMemberRequest(
    [Required]
    [MaxLength(HouseholdMember.DisplayNameMaxLength)]
    string DisplayName,
    HouseholdMemberRole Role = HouseholdMemberRole.Member);
