using System.ComponentModel.DataAnnotations;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Interface;

public record UpdateHouseholdMemberRequest(
        [Required]
        [MaxLength(HouseholdMember.DisplayNameMaxLength)]
        string DisplayName,
        [DisallowUnspecifiedHouseholdMemberRole]
        HouseholdMemberRole Role,
        bool Archived);
