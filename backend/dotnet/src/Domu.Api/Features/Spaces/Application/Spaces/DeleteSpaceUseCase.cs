using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed class DeleteSpaceUseCase(
    ISpaceRepository spaceRepository,
    ISpaceAccessService spaceAccessService,
    IUserEventRecorder? userEventRecorder = null)
    : IDeleteSpaceUseCase
{
    private readonly IUserEventRecorder _userEventRecorder = userEventRecorder ?? NoOpUserEventRecorder.Instance;

    public async Task ExecuteAsync(DeleteSpaceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await spaceAccessService.EnsureCanAccessSpaceAsync(
            command.HouseholdId,
            command.SpaceId,
            command.Actor,
            cancellationToken);

        await spaceRepository.DeleteAsync(command.SpaceId, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.Actor.ActorId,
            UserEventActions.SpaceDeleted,
            UserEventTargetTypes.Space,
            command.SpaceId,
            command.HouseholdId,
            EventMetadata.Empty(),
            cancellationToken);
        await spaceRepository.SaveChangesAsync(cancellationToken);
    }
}
