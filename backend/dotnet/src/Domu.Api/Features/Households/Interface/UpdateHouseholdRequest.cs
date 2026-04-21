using System.ComponentModel.DataAnnotations;
using Domu.Api.Features.Households.Domain.Households;

namespace Domu.Api.Features.Households.Interface;

public sealed record UpdateHouseholdRequest(
    [property: Required]
    [property: MaxLength(Household.NameMaxLength)]
    string Name);
