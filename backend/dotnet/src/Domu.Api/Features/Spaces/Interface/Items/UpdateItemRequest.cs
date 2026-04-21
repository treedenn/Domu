using System.ComponentModel.DataAnnotations;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Interface.Items;

public sealed record UpdateItemRequest(
    [property: Required]
    [property: MaxLength(Item.NameMaxLength)]
    string Name,
    [property: MaxLength(Item.CategoryMaxLength)]
    string? Category,
    [property: MaxLength(Item.BarcodeMaxLength)]
    string? Barcode,
    IReadOnlyCollection<ItemEntryRequest>? Entries);
