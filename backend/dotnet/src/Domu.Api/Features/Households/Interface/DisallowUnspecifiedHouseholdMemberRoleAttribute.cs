using System.ComponentModel.DataAnnotations;
using Domu.Api.Features.Households.Domain.Members;

namespace Domu.Api.Features.Households.Interface;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public sealed class DisallowUnspecifiedHouseholdMemberRoleAttribute : ValidationAttribute
{
    public DisallowUnspecifiedHouseholdMemberRoleAttribute()
        : base("Household member role must be specified.")
    {
    }

    public override bool IsValid(object? value)
    {
        return value is not HouseholdMemberRole.Unspecified;
    }
}
