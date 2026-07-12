using Domu.Api.Features.Spaces.Application.Items.Contracts;

namespace Domu.Api.Features.Spaces.Application.Items;

public interface IGetSpaceItemsUseCase
{
    Task<IReadOnlyList<ItemView>> ExecuteAsync(GetSpaceItemsQuery query, CancellationToken cancellationToken);
}