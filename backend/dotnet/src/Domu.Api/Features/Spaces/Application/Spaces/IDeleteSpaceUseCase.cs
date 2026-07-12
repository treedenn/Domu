namespace Domu.Api.Features.Spaces.Application.Spaces;

public interface IDeleteSpaceUseCase
{
    Task ExecuteAsync(DeleteSpaceCommand command, CancellationToken cancellationToken);
}