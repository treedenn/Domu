using Domu.Api.Features.Locations.Application.Items.Contracts;

namespace Domu.Api.Features.Locations.Application.Items;

public interface ICreateItemUseCase
{
    Task<ItemView> ExecuteAsync(CreateItemCommand command, CancellationToken cancellationToken);
}
