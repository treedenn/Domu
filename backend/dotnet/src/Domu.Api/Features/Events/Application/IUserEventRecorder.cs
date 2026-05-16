namespace Domu.Api.Features.Events.Application;

public interface IUserEventRecorder
{
    Task RecordAsync(
        Guid actorUserId,
        string action,
        string targetType,
        Guid? targetId,
        Guid? householdId,
        EventMetadata? metadata,
        CancellationToken cancellationToken);
}
