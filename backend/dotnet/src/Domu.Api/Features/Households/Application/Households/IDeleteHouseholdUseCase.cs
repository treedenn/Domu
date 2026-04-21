namespace Domu.Api.Features.Households.Application.Households;

public interface IDeleteHouseholdUseCase
{
    Task ExecuteAsync(DeleteHouseholdCommand command, CancellationToken cancellationToken);
}
