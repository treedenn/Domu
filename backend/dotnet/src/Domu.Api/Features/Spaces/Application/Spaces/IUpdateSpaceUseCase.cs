using Domu.Api.Features.Spaces.Application.Spaces.Contracts;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public interface IUpdateSpaceUseCase
{
    Task<SpaceView> ExecuteAsync(UpdateSpaceCommand command, CancellationToken cancellationToken);
}
