namespace Domu.Api.Features.Spaces.Application.Items;

public interface IDeleteItemUseCase
{
    Task ExecuteAsync(DeleteItemCommand command, CancellationToken cancellationToken);
}
