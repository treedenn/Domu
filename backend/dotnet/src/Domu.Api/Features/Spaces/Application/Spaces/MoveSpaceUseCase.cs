using Domu.Api.Features.Activities.Application;
using Domu.Api.Features.Spaces.Application.Spaces.Contracts;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed class MoveSpaceUseCase(
    ISpaceRepository spaceRepository,
    ISpaceAccessService spaceAccessService,
    IHouseholdActivityRecorder? householdActivityRecorder = null)
    : IMoveSpaceUseCase
{
    private readonly IHouseholdActivityRecorder _householdActivityRecorder =
        householdActivityRecorder ?? NoOpHouseholdActivityRecorder.Instance;

    public async Task<SpaceView> ExecuteAsync(MoveSpaceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await spaceAccessService.EnsureCanAccessSpaceAsync(
            command.HouseholdId,
            command.SpaceId,
            command.Actor,
            cancellationToken);

        if (command.ParentId is not null)
        {
            await spaceAccessService.EnsureSpaceBelongsToHouseholdAsync(
                command.ParentId.Value,
                command.HouseholdId,
                cancellationToken);

            if (await spaceRepository.IsDescendantAsync(
                    command.SpaceId,
                    command.ParentId.Value,
                    command.HouseholdId,
                    cancellationToken))
                throw new ArgumentException("Parent space cannot be the space itself or one of its descendants.",
                    nameof(command.ParentId));
        }

        var space = await spaceRepository.GetByIdAsync(command.SpaceId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Space '{command.SpaceId}' was not found.");

        space.MoveTo(command.ParentId);

        await spaceRepository.UpdateAsync(space, cancellationToken);
        await _householdActivityRecorder.RecordAsync(
            command.Actor,
            HouseholdActivityActions.SpaceMoved,
            HouseholdActivityTargetTypes.Space,
            space.Id,
            command.HouseholdId,
            ActivityMetadata.From(("parentId", space.ParentId)),
            cancellationToken);
        await spaceRepository.SaveChangesAsync(cancellationToken);

        return SpaceView.FromDomain(space);
    }
}
