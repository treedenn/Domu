using Domu.Api.Features.Spaces.Application.Items.Contracts;
using Domu.Api.Features.Spaces.Application.Items.Ports;

namespace Domu.Api.Features.Spaces.Application.Items;

public sealed class ReplaceItemEntriesUseCase(IItemRepository itemRepository) : IReplaceItemEntriesUseCase
{
    public async Task<ItemView> ExecuteAsync(ReplaceItemEntriesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var item = await itemRepository.GetByIdAsync(command.ItemId, cancellationToken)
                   ?? throw new KeyNotFoundException($"Item '{command.ItemId}' was not found.");

        ItemEntryWriter.ReplaceEntries(item, command.Entries);

        await itemRepository.UpdateAsync(item, cancellationToken);
        await itemRepository.SaveChangesAsync(cancellationToken);

        return ItemView.FromDomain(item);
    }
}
