using Domu.Api.Features.Spaces.Application.Items.Contracts;

namespace Domu.Api.Features.Spaces.Application.Items;

public interface ICreateItemUseCase
{
    Task<ItemView> ExecuteAsync(CreateItemCommand command, CancellationToken cancellationToken);
}