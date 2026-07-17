using System.ComponentModel.DataAnnotations;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Interface.Items;

public sealed record CreateItemRequest(
    [Required]
    [MaxLength(Item.NameMaxLength)]
    string Name,
    [MaxLength(Item.CategoryMaxLength)] string? Category,
    [MaxLength(Item.BarcodeMaxLength)] string? Barcode,
    IReadOnlyCollection<ItemEntryRequest>? Entries,
    int? DefaultPurchaseCount = null,
    decimal? DefaultPurchaseAmountPerUnit = null,
    ItemUnit? DefaultPurchaseUnit = null);
