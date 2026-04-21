using System.ComponentModel.DataAnnotations;
using Domu.Api.Features.Spaces.Application.Items.Contracts;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Interface.Items;

public sealed record ItemEntryRequest(
    Guid? Id,
    [property: Range(0, int.MaxValue)]
    int Quantity,
    ConsumableState State,
    DateTimeOffset? AcquisitionDate,
    DateTimeOffset? ExpirationDate)
{
    public ItemEntryDraft ToDraft()
    {
        return new ItemEntryDraft(Id, Quantity, State, AcquisitionDate, ExpirationDate);
    }
}
