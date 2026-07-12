using Domu.Api.Features.Spaces.Application.Items.Contracts;

namespace Domu.Api.Features.Spaces.Application.Items;

public interface IUpdateItemUseCase
{
    Task<ItemView> ExecuteAsync(UpdateItemCommand command, CancellationToken cancellationToken);
}