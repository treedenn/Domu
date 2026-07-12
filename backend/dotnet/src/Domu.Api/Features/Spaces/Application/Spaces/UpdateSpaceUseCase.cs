using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Spaces.Application.Spaces.Contracts;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed class UpdateSpaceUseCase(
    ISpaceRepository spaceRepository,
    ISpaceAccessService spaceAccessService,
    IHouseholdEventRecorder? userEventRecorder = null)
    : IUpdateSpaceUseCase
{
    private readonly IHouseholdEventRecorder _userEventRecorder = userEventRecorder ?? NoOpHouseholdEventRecorder.Instance;

    public async Task<SpaceView> ExecuteAsync(UpdateSpaceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await spaceAccessService.EnsureCanAccessSpaceAsync(
            command.HouseholdId,
            command.SpaceId,
            command.Actor,
            cancellationToken);

        var space = await spaceRepository.GetByIdAsync(command.SpaceId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Space '{command.SpaceId}' was not found.");

        space.Rename(command.Name);
        space.Describe(command.Description);

        await spaceRepository.UpdateAsync(space, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.Actor.ActorId,
            HouseholdEventActions.SpaceUpdated,
            HouseholdEventTargetTypes.Space,
            space.Id,
            command.HouseholdId,
            EventMetadata.From(("name", space.Name)),
            cancellationToken);
        await spaceRepository.SaveChangesAsync(cancellationToken);

        return SpaceView.FromDomain(space);
    }
}
