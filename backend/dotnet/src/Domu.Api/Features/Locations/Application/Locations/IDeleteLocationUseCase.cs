namespace Domu.Api.Features.Locations.Application.Locations;

public interface IDeleteLocationUseCase
{
    Task ExecuteAsync(DeleteLocationCommand command, CancellationToken cancellationToken);
}
