using System.ComponentModel.DataAnnotations;
using Domu.Api.Features.Spaces.Application.Items.Contracts;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Interface.Items;

public sealed record ItemEntryRequest(
    Guid? Id,
    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    decimal InitialQuantity,
    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    decimal CurrentQuantity,
    ItemUnit? Unit,
    ItemContainerType? ContainerType,
    ConsumableState State,
    DateTimeOffset? AcquisitionDate,
    DateTimeOffset? ExpirationDate)
{
    public ItemEntryDraft ToDraft()
    {
        return new ItemEntryDraft(
            Id,
            InitialQuantity,
            CurrentQuantity,
            Unit ?? ItemUnit.Piece,
            ContainerType ?? ItemContainerType.Unspecified,
            State,
            AcquisitionDate,
            ExpirationDate);
    }
}
