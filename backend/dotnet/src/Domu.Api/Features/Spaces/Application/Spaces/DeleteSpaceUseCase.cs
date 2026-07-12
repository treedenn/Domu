using Domu.Api.Features.Activities.Application;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed class DeleteSpaceUseCase(
    ISpaceRepository spaceRepository,
    ISpaceAccessService spaceAccessService,
    IHouseholdActivityRecorder? householdActivityRecorder = null)
    : IDeleteSpaceUseCase
{
    private readonly IHouseholdActivityRecorder _householdActivityRecorder =
        householdActivityRecorder ?? NoOpHouseholdActivityRecorder.Instance;

    public async Task ExecuteAsync(DeleteSpaceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await spaceAccessService.EnsureCanAccessSpaceAsync(
            command.HouseholdId,
            command.SpaceId,
            command.Actor,
            cancellationToken);

        await spaceRepository.DeleteAsync(command.SpaceId, cancellationToken);
        await _householdActivityRecorder.RecordAsync(
            command.Actor,
            HouseholdActivityActions.SpaceDeleted,
            HouseholdActivityTargetTypes.Space,
            command.SpaceId,
            command.HouseholdId,
            ActivityMetadata.Empty(),
            cancellationToken);
        await spaceRepository.SaveChangesAsync(cancellationToken);
    }
}