using Domu.Api.Features.Spaces.Application.Items.Contracts;
using Domu.Api.Features.Spaces.Application.Items.Ports;

namespace Domu.Api.Features.Spaces.Application.Items;

public sealed class GetSpaceItemsUseCase(IItemRepository itemRepository) : IGetSpaceItemsUseCase
{
    public async Task<IReadOnlyList<ItemView>> ExecuteAsync(Guid spaceId, CancellationToken cancellationToken)
    {
        var items = await itemRepository.GetBySpaceAsync(spaceId, cancellationToken);
        return items
            .Select(ItemView.FromDomain)
            .ToArray();
    }
}
