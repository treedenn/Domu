using System.ComponentModel.DataAnnotations;
using Domu.Api.Features.Households.Domain.Households;

namespace Domu.Api.Features.Households.Interface;

public sealed record CreateHouseholdRequest(
    [Required]
    [MaxLength(Household.NameMaxLength)]
    string Name);
