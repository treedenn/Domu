using Domu.Api.Features.Locations.Application.Locations.Contracts;

namespace Domu.Api.Features.Locations.Application.Locations;

public interface IGetLocationsPageUseCase
{
    Task<LocationPage> ExecuteAsync(GetLocationsPageQuery query, CancellationToken cancellationToken);
}
