namespace Domu.Api.Features.Events.Application;

public sealed class NoOpUserEventRecorder : IUserEventRecorder
{
    public static readonly NoOpUserEventRecorder Instance = new();

    private NoOpUserEventRecorder()
    {
    }

    public Task RecordAsync(
        Guid actorUserId,
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
