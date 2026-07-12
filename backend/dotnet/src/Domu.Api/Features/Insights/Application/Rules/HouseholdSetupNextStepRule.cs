using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Insights.Application.Contracts;

namespace Domu.Api.Features.Insights.Application.Rules;

/// <summary>
///     Suggests the next household setup step when a new household has no spaces yet.
/// </summary>
/// <remarks>
///     Purpose: provide lightweight onboarding guidance from the household dashboard.
///     Produces: <c>household.setup_next_step</c> insights with a <c>space.create</c> action.
///     Trigger: a recent <c>household.created</c> event exists and no <c>space.created</c> event exists.
///     Dedupe: one setup prompt per household using <c>space.create:household:{householdId}</c>.
/// </remarks>
public sealed class HouseholdSetupNextStepRule : IInsightRule
{
    public string Key => "household-setup-next-step";

    public Task<IReadOnlyList<HouseholdInsightCandidate>> EvaluateAsync(
        InsightContext context,
        CancellationToken cancellationToken)
    {
        var householdCreated =
            context.Events.Any(userEvent => userEvent.Action == HouseholdEventActions.HouseholdCreated);
        var spaceCreated = context.Events.Any(userEvent => userEvent.Action == HouseholdEventActions.SpaceCreated);
        if (!householdCreated || spaceCreated)
            return Task.FromResult<IReadOnlyList<HouseholdInsightCandidate>>([]);

        var insight = new HouseholdInsight(
            $"household-setup-next-step:{context.HouseholdId}",
            InsightTypes.HouseholdSetupNextStep,
            "Create the first space",
            "Start organizing this household by adding a room, shelf, or storage area.",
            0.7,
            30,
            Key,
            InsightTargetTypes.Household,
            context.HouseholdId,
            new InsightAction(
                InsightActionTypes.CreateSpace,
                InsightTargetTypes.Household,
                context.HouseholdId,
                new Dictionary<string, object?>()),
            new Dictionary<string, object?>());

        return Task.FromResult<IReadOnlyList<HouseholdInsightCandidate>>(
            [new HouseholdInsightCandidate($"space.create:household:{context.HouseholdId}", insight)]);
    }
}