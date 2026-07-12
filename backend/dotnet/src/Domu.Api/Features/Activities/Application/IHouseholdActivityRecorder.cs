using Domu.Api.Features.Auth.Domain;

namespace Domu.Api.Features.Activities.Application;

public interface IHouseholdActivityRecorder
{
    Task RecordAsync(
        DomuActor actor,
        string action,
        string targetType,
        Guid? targetId,
        Guid householdId,
        ActivityMetadata? metadata,
        CancellationToken cancellationToken);
}
