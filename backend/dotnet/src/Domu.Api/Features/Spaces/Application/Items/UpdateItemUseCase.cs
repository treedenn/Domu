using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Spaces.Application.Items.Contracts;
using Domu.Api.Features.Spaces.Application.Items.Ports;
using Domu.Api.Features.Spaces.Application.Spaces;

namespace Domu.Api.Features.Spaces.Application.Items;

public sealed class UpdateItemUseCase(
    IItemRepository itemRepository,
    ISpaceAccessService spaceAccessService,
    IHouseholdEventRecorder? userEventRecorder = null)
    : IUpdateItemUseCase
{
    private readonly IHouseholdEventRecorder _userEventRecorder =
        userEventRecorder ?? NoOpHouseholdEventRecorder.Instance;

    public async Task<ItemView> ExecuteAsync(UpdateItemCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await spaceAccessService.EnsureCanAccessSpaceAsync(
            command.HouseholdId,
            command.SpaceId,
            command.Actor,
            cancellationToken);

        var item = await itemRepository.GetByIdAsync(command.ItemId, cancellationToken)
                   ?? throw new KeyNotFoundException($"Item '{command.ItemId}' was not found.");
        if (item.SpaceId != command.SpaceId)
            throw new KeyNotFoundException($"Item '{command.ItemId}' was not found.");

        item.Rename(command.Name);
        item.ChangeCategory(command.Category);
        item.ChangeBarcode(command.Barcode);

        await itemRepository.UpdateAsync(item, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.Actor.ActorId,
            HouseholdEventActions.ItemUpdated,
            HouseholdEventTargetTypes.Item,
            item.Id,
            command.HouseholdId,
            EventMetadata.From(
                ("spaceId", command.SpaceId),
                ("name", item.Name),
                ("category", item.Category),
                ("barcode", item.Barcode)),
            cancellationToken);
        await itemRepository.SaveChangesAsync(cancellationToken);

        return ItemView.FromDomain(item);
    }
}