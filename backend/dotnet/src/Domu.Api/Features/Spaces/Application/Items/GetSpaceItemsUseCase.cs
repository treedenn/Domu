using Domu.Api.Features.Spaces.Application.Items.Contracts;
using Domu.Api.Features.Spaces.Application.Items.Ports;
using Domu.Api.Features.Spaces.Application.Spaces;

namespace Domu.Api.Features.Spaces.Application.Items;

public sealed class GetSpaceItemsUseCase(
    IItemRepository itemRepository,
    ISpaceAccessService spaceAccessService)
    : IGetSpaceItemsUseCase
{
    public async Task<IReadOnlyList<ItemView>> ExecuteAsync(GetSpaceItemsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        await spaceAccessService.EnsureCanAccessSpaceAsync(
            query.HouseholdId,
            query.SpaceId,
            query.Actor,
            cancellationToken);

        var items = await itemRepository.GetBySpaceAsync(query.SpaceId, cancellationToken);
        return items
            .Select(ItemView.FromDomain)
            .ToArray();
    }
}