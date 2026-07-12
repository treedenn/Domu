using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Spaces.Application.Spaces.Contracts;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;
using Domu.Api.Features.Spaces.Domain.Spaces;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed class CreateSpaceUseCase(
    ISpaceRepository spaceRepository,
    ISpaceAccessService spaceAccessService,
    IHouseholdEventRecorder? userEventRecorder = null)
    : ICreateSpaceUseCase
{
    private readonly IHouseholdEventRecorder _userEventRecorder =
        userEventRecorder ?? NoOpHouseholdEventRecorder.Instance;

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
        await _userEventRecorder.RecordAsync(
            command.Actor.ActorId,
            HouseholdEventActions.SpaceCreated,
            HouseholdEventTargetTypes.Space,
            space.Id,
            command.HouseholdId,
            EventMetadata.From(("name", space.Name), ("parentId", space.ParentId)),
            cancellationToken);
        await spaceRepository.SaveChangesAsync(cancellationToken);

        return SpaceView.FromDomain(space);
    }
}