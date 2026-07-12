namespace Domu.Api.Features.Spaces.Domain.Items;

public static class ItemUnitExtensions
{
    public static ItemUnitKind GetKind(this ItemUnit unit)
    {
        return unit switch
        {
            ItemUnit.Piece => ItemUnitKind.Count,
            ItemUnit.Milliliter or ItemUnit.Liter => ItemUnitKind.Volume,
            ItemUnit.Gram or ItemUnit.Kilogram => ItemUnitKind.Mass,
            _ => ItemUnitKind.Unspecified
        };
    }
}