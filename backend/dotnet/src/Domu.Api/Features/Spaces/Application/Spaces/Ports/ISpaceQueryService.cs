using Domu.Api.Features.Spaces.Application.Spaces.Contracts;

namespace Domu.Api.Features.Spaces.Application.Spaces.Ports;

public interface ISpaceQueryService
{
    Task<SpacePage> GetPageAsync(GetSpacesPageQuery query, CancellationToken cancellationToken);
}