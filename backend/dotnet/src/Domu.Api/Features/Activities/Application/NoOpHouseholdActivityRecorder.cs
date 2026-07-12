using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Activities.Application;

public sealed class NoOpHouseholdActivityRecorder : IHouseholdActivityRecorder
{
    public static readonly NoOpHouseholdActivityRecorder Instance = new();

    private NoOpHouseholdActivityRecorder()
    {
    }

    public Task RecordAsync(
        DomuActor actor,
        string action,
        string targetType,
        Guid? targetId,
        Guid householdId,
        ActivityMetadata? metadata,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
