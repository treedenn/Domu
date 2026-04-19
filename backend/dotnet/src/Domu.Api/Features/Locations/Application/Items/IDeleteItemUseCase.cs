namespace Domu.Api.Features.Locations.Application.Items;

public interface IDeleteItemUseCase
{
    Task ExecuteAsync(DeleteItemCommand command, CancellationToken cancellationToken);
}
