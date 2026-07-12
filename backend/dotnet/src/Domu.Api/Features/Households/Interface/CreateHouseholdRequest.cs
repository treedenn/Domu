using System.ComponentModel.DataAnnotations;
using Domu.Api.Features.Households.Domain.Households;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Interface;

public sealed record CreateHouseholdRequest(
    [Required]
    [MaxLength(Household.NameMaxLength)]
    string Name,
    [Required]
    [MaxLength(HouseholdMember.DisplayNameMaxLength)]
    string OwnerDisplayName);