using System.Text.Json;
using System.Text.Json.Serialization;
using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Events.Domain;
using Domu.Api.Infrastructure.Database;
using Domu.Api.Interface.RequestContext;

namespace Domu.Api.Features.Events.Infrastructure;

public sealed class UserEventRecorder(
    AppDbContext dbContext,
    IClientRequestContextAccessor clientRequestContextAccessor)
    : IUserEventRecorder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task RecordAsync(
        Guid actorUserId,
        string action,
        string targetType,
        Guid? targetId,
        Guid? householdId,
        EventMetadata? metadata,
        CancellationToken cancellationToken)
    {
        var client = clientRequestContextAccessor.Current;
        var userEvent = new UserEvent(
            Guid.CreateVersion7(),
            DateTimeOffset.UtcNow,
            actorUserId,
            action,
            targetType,
            targetId,
            householdId,
            JsonSerializer.Serialize(metadata ?? EventMetadata.Empty(), JsonOptions),
            client.RequestId,
            client.App,
            client.Platform,
            client.VersionRaw,
            client.Build);

        await dbContext.UserEvents.AddAsync(new UserEventEntity(userEvent), cancellationToken);
    }
}
