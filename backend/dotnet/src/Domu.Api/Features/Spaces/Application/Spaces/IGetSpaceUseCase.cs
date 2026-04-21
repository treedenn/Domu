using Domu.Api.Features.Spaces.Application.Spaces.Contracts;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public interface IGetSpaceUseCase
{
    Task<SpaceView> ExecuteAsync(GetSpaceQuery query, CancellationToken cancellationToken);
}
