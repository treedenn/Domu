using Domu.Api.Features.Locations.Application.Items.Contracts;

namespace Domu.Api.Features.Locations.Application.Items;

public interface IGetLocationItemsUseCase
{
    Task<IReadOnlyList<ItemView>> ExecuteAsync(Guid locationId, CancellationToken cancellationToken);
}
