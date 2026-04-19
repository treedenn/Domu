using Domu.Api.Features.Locations.Application.Items.Contracts;

namespace Domu.Api.Features.Locations.Application.Items;

public interface IUpdateItemUseCase
{
    Task<ItemView> ExecuteAsync(UpdateItemCommand command, CancellationToken cancellationToken);
}
