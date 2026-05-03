using Domu.Api.Features.Spaces.Application.Spaces.Contracts;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed class MoveSpaceUseCase(
    ISpaceRepository spaceRepository,
    ISpaceAccessService spaceAccessService)
    : IMoveSpaceUseCase
{
    public async Task<SpaceView> ExecuteAsync(MoveSpaceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await spaceAccessService.EnsureCanAccessSpaceAsync(
            command.HouseholdId,
            command.SpaceId,
            command.UserId,
            cancellationToken);

        if (command.ParentId is not null)
            await spaceAccessService.EnsureSpaceBelongsToHouseholdAsync(
                command.ParentId.Value,
                command.HouseholdId,
                cancellationToken);

        var space = await spaceRepository.GetByIdAsync(command.SpaceId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Space '{command.SpaceId}' was not found.");

        space.MoveTo(command.ParentId);

        await spaceRepository.UpdateAsync(space, cancellationToken);
        await spaceRepository.SaveChangesAsync(cancellationToken);

        return SpaceView.FromDomain(space);
    }
}
