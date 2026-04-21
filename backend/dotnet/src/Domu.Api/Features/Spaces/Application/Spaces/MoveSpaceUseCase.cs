using Domu.Api.Features.Spaces.Application.Spaces.Contracts;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed class MoveSpaceUseCase(ISpaceRepository spaceRepository) : IMoveSpaceUseCase
{
    public async Task<SpaceView> ExecuteAsync(MoveSpaceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var space = await spaceRepository.GetByIdAsync(command.SpaceId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Space '{command.SpaceId}' was not found.");

        space.MoveTo(command.ParentId);

        await spaceRepository.UpdateAsync(space, cancellationToken);
        await spaceRepository.SaveChangesAsync(cancellationToken);

        return SpaceView.FromDomain(space);
    }
}
