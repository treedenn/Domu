using Domu.Api.Features.Locations.Application.Locations.Contracts;

namespace Domu.Api.Features.Locations.Application.Locations;

public interface IUpdateLocationUseCase
{
    Task<LocationView> ExecuteAsync(UpdateLocationCommand command, CancellationToken cancellationToken);
}
