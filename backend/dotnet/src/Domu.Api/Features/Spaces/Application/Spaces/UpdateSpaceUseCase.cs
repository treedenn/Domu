using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Spaces.Application.Spaces.Contracts;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed class UpdateSpaceUseCase(
    ISpaceRepository spaceRepository,
    ISpaceAccessService spaceAccessService,
    IUserEventRecorder? userEventRecorder = null)
    : IUpdateSpaceUseCase
{
    private readonly IUserEventRecorder _userEventRecorder = userEventRecorder ?? NoOpUserEventRecorder.Instance;

    public async Task<SpaceView> ExecuteAsync(UpdateSpaceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await spaceAccessService.EnsureCanAccessSpaceAsync(
            command.HouseholdId,
            command.SpaceId,
            command.UserId,
            cancellationToken);

        var space = await spaceRepository.GetByIdAsync(command.SpaceId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Space '{command.SpaceId}' was not found.");

        space.Rename(command.Name);
        space.Describe(command.Description);

        await spaceRepository.UpdateAsync(space, cancellationToken);
        await _userEventRecorder.RecordAsync(
            command.UserId,
            UserEventActions.SpaceUpdated,
            UserEventTargetTypes.Space,
            space.Id,
            command.HouseholdId,
            EventMetadata.From(("name", space.Name)),
            cancellationToken);
        await spaceRepository.SaveChangesAsync(cancellationToken);

        return SpaceView.FromDomain(space);
    }
}
