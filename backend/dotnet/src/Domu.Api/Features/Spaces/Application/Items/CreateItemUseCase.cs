using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Spaces.Application.Items.Contracts;
using Domu.Api.Features.Spaces.Application.Items.Ports;
using Domu.Api.Features.Spaces.Application.Spaces;
using Domu.Api.Features.Spaces.Domain.Items;

namespace Domu.Api.Features.Spaces.Application.Items;

public sealed class CreateItemUseCase(
    IItemRepository itemRepository,
    ISpaceAccessService spaceAccessService,
    IUserEventRecorder? userEventRecorder = null)
    : ICreateItemUseCase
{
    private readonly IUserEventRecorder _userEventRecorder = userEventRecorder ?? NoOpUserEventRecorder.Instance;

    public async Task<ItemView> ExecuteAsync(CreateItemCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await spaceAccessService.EnsureCanAccessSpaceAsync(
            command.HouseholdId,
            command.SpaceId,
            command.Actor,
            cancellationToken);

        var item = new Item(Guid.CreateVersion7(), command.Name, command.SpaceId);
        item.ChangeCategory(command.Category);
        item.ChangeBarcode(command.Barcode);
        ItemEntryWriter.ReplaceEntries(item, command.Entries);

        await itemRepository.AddAsync(item, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.Actor.ActorId,
            UserEventActions.ItemCreated,
            UserEventTargetTypes.Item,
            item.Id,
            command.HouseholdId,
            EventMetadata.From(
                ("spaceId", command.SpaceId),
                ("name", item.Name),
                ("category", item.Category),
                ("barcode", item.Barcode),
                ("entryCount", item.Entries.Count)),
            cancellationToken);
        await itemRepository.SaveChangesAsync(cancellationToken);

        return ItemView.FromDomain(item);
    }
}
