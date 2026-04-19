using Domu.Api.Features.Locations.Application.Items.Contracts;
using Domu.Api.Features.Locations.Application.Items.Ports;

namespace Domu.Api.Features.Locations.Application.Items;

public sealed class GetLocationItemsUseCase(IItemRepository itemRepository) : IGetLocationItemsUseCase
{
    public async Task<IReadOnlyList<ItemView>> ExecuteAsync(Guid locationId, CancellationToken cancellationToken)
    {
        var items = await itemRepository.GetByLocationAsync(locationId, cancellationToken);
        return items
            .Select(ItemView.FromDomain)
            .ToArray();
    }
}
