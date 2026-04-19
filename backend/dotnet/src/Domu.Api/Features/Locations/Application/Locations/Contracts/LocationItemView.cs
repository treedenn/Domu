namespace Domu.Api.Features.Locations.Application.Locations.Contracts;

public sealed record LocationItemView(
    Guid Id,
    Guid LocationId,
    string Name,
    string? Category,
    string? Barcode,
    int TotalQuantity);
