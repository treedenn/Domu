using Domu.Api.Features.Locations.Application.Items.Ports;

namespace Domu.Api.Features.Locations.Application.Items;

public sealed class DeleteItemUseCase(IItemRepository itemRepository) : IDeleteItemUseCase
{
    public async Task ExecuteAsync(DeleteItemCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        await itemRepository.DeleteAsync(command.ItemId, cancellationToken);
        await itemRepository.SaveChangesAsync(cancellationToken);
    }
}
