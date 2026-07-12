namespace Domu.Api.Features.Events.Application;

public interface IHouseholdEventRecorder
{
    Task RecordAsync(
        Guid actorMemberId,
        string action,
        string targetType,
        Guid? targetId,
        Guid? householdId,
        EventMetadata? metadata,
        CancellationToken cancellationToken);
}
