using Domu.Api.Features.Auth.Application;
using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.Households.Application.Households.Contracts;
using Domu.Api.Features.Households.Application.Members;
using Domu.Api.Features.Households.Application.Members.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Domu.Api.Features.Households.Interface;

[ApiController]
[Authorize]
[Route("households")]
[Tags("Households")]
public sealed class HouseholdsController(
    IActorAccessor actorAccessor,
    CreateHouseholdUseCase createHouseholdUseCase,
    GetHouseholdUseCase getHouseholdUseCase,
    GetHouseholdsUseCase getHouseholdsUseCase,
    UpdateHouseholdUseCase updateHouseholdUseCase,
    DeleteHouseholdUseCase deleteHouseholdUseCase,
    GetHouseholdMembersUseCase getHouseholdMembersUseCase,
    UpdateHouseholdMemberUseCase updateHouseholdMemberUseCase,
    GetHouseholdInvitationsUseCase getHouseholdInvitationsUseCase,
    InviteHouseholdMemberUseCase inviteHouseholdMemberUseCase,
    AcceptHouseholdInvitationUseCase acceptHouseholdInvitationUseCase)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HouseholdView>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HouseholdView>>> GetHouseholds(CancellationToken cancellationToken)
    {
        var households = await getHouseholdsUseCase.ExecuteAsync(
            new GetHouseholdsQuery(actorAccessor.DomuActor),
            cancellationToken);

        return Ok(households);
    }

    [HttpGet("{householdId:guid}/members")]
    [ProducesResponseType(typeof(IReadOnlyList<HouseholdMemberView>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<HouseholdMemberView>>> GetMembers(
        Guid householdId,
        CancellationToken cancellationToken)
    {
        try
        {
            var members = await getHouseholdMembersUseCase.ExecuteAsync(
                new GetHouseholdMembersQuery(householdId, actorAccessor.DomuActor),
                cancellationToken);

            return Ok(members);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("{householdId:guid}/members/{memberId:guid}")]
    [ProducesResponseType(typeof(HouseholdMemberView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HouseholdMemberView>> UpdateMember(
        Guid householdId,
        Guid memberId,
        UpdateHouseholdMemberRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var member = await updateHouseholdMemberUseCase.ExecuteAsync(
                new UpdateHouseholdMemberCommand(
                    actorAccessor.DomuActor,
                    householdId,
                    memberId,
                    request.DisplayName,
                    request.Role,
                    request.Archived),
                cancellationToken);

            return Ok(member);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid household member.", Detail = exception.Message });
        }
    }

    [HttpGet("{householdId:guid}/invitations")]
    [ProducesResponseType(typeof(IReadOnlyList<HouseholdInvitationView>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<HouseholdInvitationView>>> GetInvitations(
        Guid householdId,
        CancellationToken cancellationToken)
    {
        try
        {
            var invitations = await getHouseholdInvitationsUseCase.ExecuteAsync(
                new GetHouseholdInvitationsQuery(householdId, actorAccessor.DomuActor),
                cancellationToken);

            return Ok(invitations);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{householdId:guid}/invitations")]
    [ProducesResponseType(typeof(HouseholdInvitationView), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HouseholdInvitationView>> InviteMember(
        Guid householdId,
        InviteHouseholdMemberRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var invitation = await inviteHouseholdMemberUseCase.ExecuteAsync(
                new InviteHouseholdMemberCommand(actorAccessor.DomuActor, householdId, request.Email,
                    request.DisplayName, request.Role),
                cancellationToken);

            return Created($"/api/v1/households/{householdId}/invitations/{invitation.Id}", invitation);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid invitation.", Detail = exception.Message });
        }
    }

    [HttpPost("invitations/{token}/accept")]
    [ProducesResponseType(typeof(HouseholdMemberView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HouseholdMemberView>> AcceptInvitation(
        string token,
        CancellationToken cancellationToken)
    {
        try
        {
            var member = await acceptHouseholdInvitationUseCase.ExecuteAsync(
                new AcceptHouseholdInvitationCommand(actorAccessor.DomuActor, token),
                cancellationToken);

            return Ok(member);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid invitation.", Detail = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid invitation.", Detail = exception.Message });
        }
    }

    [HttpGet("{householdId:guid}")]
    [ProducesResponseType(typeof(HouseholdView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HouseholdView>> GetHousehold(
        Guid householdId,
        CancellationToken cancellationToken)
    {
        try
        {
            var household = await getHouseholdUseCase.ExecuteAsync(
                new GetHouseholdQuery(householdId, actorAccessor.DomuActor),
                cancellationToken);

            return Ok(household);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(HouseholdView), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HouseholdView>> CreateHousehold(
        CreateHouseholdRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var household = await createHouseholdUseCase.ExecuteAsync(
                new CreateHouseholdCommand(actorAccessor.DomuActor, request.Name, request.OwnerDisplayName),
                cancellationToken);

            return CreatedAtAction(nameof(GetHousehold), new { householdId = household.Id }, household);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid household.", Detail = exception.Message });
        }
    }

    [HttpPut("{householdId:guid}")]
    [ProducesResponseType(typeof(HouseholdView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HouseholdView>> UpdateHousehold(
        Guid householdId,
        UpdateHouseholdRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var household = await updateHouseholdUseCase.ExecuteAsync(
                new UpdateHouseholdCommand(householdId, actorAccessor.DomuActor, request.Name),
                cancellationToken);

            return Ok(household);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid household.", Detail = exception.Message });
        }
    }

    [HttpDelete("{householdId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteHousehold(Guid householdId, CancellationToken cancellationToken)
    {
        try
        {
            await deleteHouseholdUseCase.ExecuteAsync(
                new DeleteHouseholdCommand(householdId, actorAccessor.DomuActor),
                cancellationToken);

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}