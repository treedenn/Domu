using Domu.Api.Features.Spaces.Application.Spaces.Contracts;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public interface ICreateSpaceUseCase
{
    Task<SpaceView> ExecuteAsync(CreateSpaceCommand command, CancellationToken cancellationToken);
}
