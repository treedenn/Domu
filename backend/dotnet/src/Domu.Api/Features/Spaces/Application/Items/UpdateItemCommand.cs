namespace Domu.Api.Features.Spaces.Application.Items;

public sealed record UpdateItemCommand(
    Guid ItemId,
    string Name,
    string? Category,
    string? Barcode);
