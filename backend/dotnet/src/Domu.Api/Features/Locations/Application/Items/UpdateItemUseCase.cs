using Domu.Api.Features.Locations.Application.Items.Contracts;
using Domu.Api.Features.Locations.Application.Items.Ports;

namespace Domu.Api.Features.Locations.Application.Items;

public sealed class UpdateItemUseCase(IItemRepository itemRepository) : IUpdateItemUseCase
{
    public async Task<ItemView> ExecuteAsync(UpdateItemCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var item = await itemRepository.GetByIdAsync(command.ItemId, cancellationToken)
                   ?? throw new KeyNotFoundException($"Item '{command.ItemId}' was not found.");

        item.Rename(command.Name);
        item.MoveTo(command.LocationId);
        item.ChangeCategory(command.Category);
        item.ChangeBarcode(command.Barcode);
        ItemEntryWriter.ReplaceEntries(item, command.Entries);

        await itemRepository.UpdateAsync(item, cancellationToken);
        await itemRepository.SaveChangesAsync(cancellationToken);

        return ItemView.FromDomain(item);
    }
}
