namespace Domu.Api.Features.Spaces.Application.Spaces.Contracts;

public sealed record SpaceItemView(
    Guid Id,
    Guid SpaceId,
    string Name,
    string? Category,
    string? Barcode,
    decimal TotalQuantity);