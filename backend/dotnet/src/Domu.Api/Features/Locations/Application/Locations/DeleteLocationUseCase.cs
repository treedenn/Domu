using Domu.Api.Features.Locations.Application.Locations.Ports;

namespace Domu.Api.Features.Locations.Application.Locations;

public sealed class DeleteLocationUseCase(ILocationRepository locationRepository) : IDeleteLocationUseCase
{
    public async Task ExecuteAsync(DeleteLocationCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        await locationRepository.DeleteAsync(command.LocationId, cancellationToken);
        await locationRepository.SaveChangesAsync(cancellationToken);
    }
}
