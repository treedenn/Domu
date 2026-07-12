using System.Text.Json;
using System.Text.Json.Serialization;
using Domu.Api.Features.Activities.Application;
using Domu.Api.Features.Activities.Domain;
using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Households.Infrastructure.Members;
using Domu.Api.Infrastructure.Database;
using Domu.Api.Interface.RequestContext;
using Microsoft.EntityFrameworkCore;

namespace Domu.Api.Features.Activities.Infrastructure;

public sealed class HouseholdActivityRecorder(
    AppDbContext dbContext,
    IClientRequestContextAccessor clientRequestContextAccessor)
    : IHouseholdActivityRecorder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task RecordAsync(
        DomuActor actor,
        string action,
        string targetType,
        Guid? targetId,
        Guid householdId,
        ActivityMetadata? metadata,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actor);
        if (householdId == Guid.Empty)
            throw new ArgumentException("Household activities require a household id.", nameof(householdId));

        var actorMember = FindTrackedActorMember(actor, householdId)
                          ?? await FindPersistedActorMemberAsync(actor, householdId, cancellationToken)
                          ?? throw new KeyNotFoundException(
                              $"Actor '{actor.ActorId}' is not an active member of household '{householdId}'.");

        var client = clientRequestContextAccessor.Current;
        var householdActivity = new HouseholdActivity(
            Guid.CreateVersion7(),
            DateTimeOffset.UtcNow,
            actor.ActorId,
            actorMember.Id,
            action,
            targetType,
            targetId,
            householdId,
            JsonSerializer.Serialize(metadata ?? ActivityMetadata.Empty(), JsonOptions),
            client.RequestId,
            client.App,
            client.Platform,
            client.VersionRaw,
            client.Build);

        await dbContext.HouseholdActivities.AddAsync(new HouseholdActivityEntity(householdActivity), cancellationToken);
    }

    private HouseholdMemberEntity? FindTrackedActorMember(DomuActor actor, Guid householdId)
    {
        return dbContext.HouseholdMembers.Local.SingleOrDefault(member =>
            IsActorMember(member, actor, householdId));
    }

    private Task<HouseholdMemberEntity?> FindPersistedActorMemberAsync(
        DomuActor actor,
        Guid householdId,
        CancellationToken cancellationToken)
    {
        return actor.ActorType switch
        {
            DomuActorType.Zitadel => dbContext.HouseholdMembers.SingleOrDefaultAsync(
                member => member.HouseholdId == householdId
                          && member.UserId == actor.ActorId
                          && !member.Archived,
                cancellationToken),
            DomuActorType.HouseholdMember => dbContext.HouseholdMembers.SingleOrDefaultAsync(
                member => member.HouseholdId == householdId
                          && member.Id == actor.ActorId
                          && !member.Archived,
                cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(actor), actor.ActorType, "Unsupported actor type.")
        };
    }

    private static bool IsActorMember(HouseholdMemberEntity member, DomuActor actor, Guid householdId)
    {
        if (member.HouseholdId != householdId || member.Archived)
            return false;

        return actor.ActorType switch
        {
            DomuActorType.Zitadel => member.UserId == actor.ActorId,
            DomuActorType.HouseholdMember => member.Id == actor.ActorId,
            _ => throw new ArgumentOutOfRangeException(nameof(actor), actor.ActorType, "Unsupported actor type.")
        };
    }
}
