namespace Domu.Api.Features.Events.Application;

public sealed class NoOpHouseholdEventRecorder : IHouseholdEventRecorder
{
    public static readonly NoOpHouseholdEventRecorder Instance = new();

    private NoOpHouseholdEventRecorder()
    {
    }

    public Task RecordAsync(
        Guid actorMemberId,
        string action,
        string targetType,
        Guid? targetId,
        Guid? householdId,
        EventMetadata? metadata,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
