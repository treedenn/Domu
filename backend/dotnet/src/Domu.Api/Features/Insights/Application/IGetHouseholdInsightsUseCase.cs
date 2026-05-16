using Domu.Api.Features.Insights.Application.Contracts;

namespace Domu.Api.Features.Insights.Application;

public interface IGetHouseholdInsightsUseCase
{
    Task<HouseholdInsightsView> ExecuteAsync(GetHouseholdInsightsQuery query, CancellationToken cancellationToken);
}
