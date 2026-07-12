using Domu.Api.Features.Activities.Application;
using Domu.Api.Features.Spaces.Application.Spaces.Contracts;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;
using Domu.Api.Features.Spaces.Domain.Spaces;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed class CreateSpaceUseCase(
    ISpaceRepository spaceRepository,
    ISpaceAccessService spaceAccessService,
    IHouseholdActivityRecorder? householdActivityRecorder = null)
    : ICreateSpaceUseCase
{
    private readonly IHouseholdActivityRecorder _householdActivityRecorder =
        householdActivityRecorder ?? NoOpHouseholdActivityRecorder.Instance;

    public async Task<SpaceView> ExecuteAsync(CreateSpaceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await spaceAccessService.EnsureCanAccessSpaceTargetAsync(
            command.HouseholdId,
            command.ParentId,
            command.Actor,
            cancellationToken);

        var space = new Space(Guid.CreateVersion7(), command.Name, command.HouseholdId);
        space.Describe(command.Description);
        space.MoveTo(command.ParentId);

        await spaceRepository.AddAsync(space, cancellationToken);
        await _householdActivityRecorder.RecordAsync(
            command.Actor,
            HouseholdActivityActions.SpaceCreated,
            HouseholdActivityTargetTypes.Space,
            space.Id,
            command.HouseholdId,
            ActivityMetadata.From(("name", space.Name), ("parentId", space.ParentId)),
            cancellationToken);
        await spaceRepository.SaveChangesAsync(cancellationToken);

        return SpaceView.FromDomain(space);
    }
}