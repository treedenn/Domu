using Domu.Api.Features.Spaces.Application.Spaces.Contracts;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;

namespace Domu.Api.Features.Spaces.Application.Spaces;

public sealed class GetSpaceUseCase(ISpaceRepository spaceRepository) : IGetSpaceUseCase
{
    public async Task<SpaceView> ExecuteAsync(GetSpaceQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var space = await spaceRepository.GetByIdAsync(query.SpaceId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Space '{query.SpaceId}' was not found.");

        if (space.HouseholdId != query.HouseholdId)
            throw new KeyNotFoundException($"Space '{query.SpaceId}' was not found.");

        return SpaceView.FromDomain(space);
    }
}
