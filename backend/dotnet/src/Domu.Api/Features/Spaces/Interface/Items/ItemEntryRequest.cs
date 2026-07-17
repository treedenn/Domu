using System.ComponentModel.DataAnnotations;
using Domu.Api.Features.Spaces.Application.Items.Contracts;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Interface.Items;

public sealed record ItemEntryRequest(
    Guid? Id,
    [Range(1, int.MaxValue)] int Count,
    [Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal? OriginalAmountPerUnit,
    [Range(typeof(decimal), "0", "79228162514264337593543950335")] decimal? CurrentAmountPerUnit,
    ItemUnit? Unit,
    ConsumableState State,
    DateTimeOffset? AcquisitionDate,
    DateTimeOffset? ExpirationDate)
{
    public ItemEntryDraft ToDraft()
    {
        return new ItemEntryDraft(
            Id,
            Count,
            OriginalAmountPerUnit,
            CurrentAmountPerUnit,
            Unit ?? ItemUnit.Unspecified,
            State,
            AcquisitionDate,
            ExpirationDate);
    }
}
