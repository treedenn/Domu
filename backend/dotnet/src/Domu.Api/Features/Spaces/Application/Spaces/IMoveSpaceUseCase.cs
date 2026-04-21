using Domu.Api.Features.Spaces.Application.Spaces.Contracts;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public interface IMoveSpaceUseCase
{
    Task<SpaceView> ExecuteAsync(MoveSpaceCommand command, CancellationToken cancellationToken);
}
