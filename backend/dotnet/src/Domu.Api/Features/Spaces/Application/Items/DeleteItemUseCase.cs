using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Spaces.Application.Items.Ports;
using Domu.Api.Features.Spaces.Application.Spaces;

namespace Domu.Api.Features.Spaces.Application.Items;

public sealed class DeleteItemUseCase(
    IItemRepository itemRepository,
    ISpaceAccessService spaceAccessService,
    IHouseholdEventRecorder? userEventRecorder = null)
    : IDeleteItemUseCase
{
    private readonly IHouseholdEventRecorder _userEventRecorder =
        userEventRecorder ?? NoOpHouseholdEventRecorder.Instance;

    public async Task ExecuteAsync(DeleteItemCommand command, CancellationToken cancellationToken)
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

        await itemRepository.DeleteAsync(command.ItemId, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.Actor.ActorId,
            HouseholdEventActions.ItemDeleted,
            HouseholdEventTargetTypes.Item,
            command.ItemId,
            command.HouseholdId,
            EventMetadata.From(("spaceId", command.SpaceId), ("name", item.Name)),
            cancellationToken);
        await itemRepository.SaveChangesAsync(cancellationToken);
    }
}