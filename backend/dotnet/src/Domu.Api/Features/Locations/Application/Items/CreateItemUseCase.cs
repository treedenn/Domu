using Domu.Api.Features.Locations.Application.Items.Contracts;
using Domu.Api.Features.Locations.Application.Items.Ports;
using Domu.Api.Features.Locations.Domain.Items;

namespace Domu.Api.Features.Locations.Application.Items;

public sealed class CreateItemUseCase(IItemRepository itemRepository) : ICreateItemUseCase
{
    public async Task<ItemView> ExecuteAsync(CreateItemCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var item = new Item(Guid.CreateVersion7(), command.Name, command.LocationId);
        item.ChangeCategory(command.Category);
        item.ChangeBarcode(command.Barcode);
        ItemEntryWriter.ReplaceEntries(item, command.Entries);

        await itemRepository.AddAsync(item, cancellationToken);
        await itemRepository.SaveChangesAsync(cancellationToken);

        return ItemView.FromDomain(item);
    }
}
