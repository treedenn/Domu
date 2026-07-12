using System.Text.Json;
using System.Text.Json.Serialization;
using Domu.Api.Features.Events.Application;
using Domu.Api.Features.Events.Domain;
using Domu.Api.Infrastructure.Database;
using Domu.Api.Interface.RequestContext;
using Microsoft.EntityFrameworkCore;

namespace Domu.Api.Features.Events.Infrastructure;

public sealed class HouseholdEventRecorder(
    AppDbContext dbContext,
    IClientRequestContextAccessor clientRequestContextAccessor)
    : IHouseholdEventRecorder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task RecordAsync(
        Guid actorMemberId,
        string action,
        string targetType,
        Guid? targetId,
        Guid? householdId,
        EventMetadata? metadata,
        CancellationToken cancellationToken)
    {
        if (householdId is null)
            throw new ArgumentException("Household events require a household id.", nameof(householdId));

        var actor = await dbContext.HouseholdMembers
                        .SingleOrDefaultAsync(
                            member => member.HouseholdId == householdId.Value
                                      && (member.Id == actorMemberId || member.UserId == actorMemberId),
                            cancellationToken)
                    ?? throw new KeyNotFoundException(
                        $"Actor '{actorMemberId}' is not a member of household '{householdId}'.");

        var client = clientRequestContextAccessor.Current;
        var userEvent = new HouseholdEvent(
            Guid.CreateVersion7(),
            DateTimeOffset.UtcNow,
            actor.Id,
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

        await dbContext.HouseholdEvents.AddAsync(new HouseholdEventEntity(userEvent), cancellationToken);
    }
}