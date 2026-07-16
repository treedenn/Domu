using Domu.Api.Features.Auth.Application;
using Domu.Api.Features.Auth.Domain;
using Domu.Api.Features.Spaces.Application.Spaces;
using Domu.Api.Features.Spaces.Application.Spaces.Contracts;
using Domu.Api.Features.Spaces.Interface.Spaces;
using Domu.Api.Interface.Responses;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Domu.Tests.Features.Spaces.Interface;

public sealed class SpacesControllerTests
{
    [Fact]
    public async Task MoveSpace_WhenMoveIsInvalid_ReturnsBadRequestProblemDetails()
    {
        var moveSpaceUseCase = Substitute.For<IMoveSpaceUseCase>();
        moveSpaceUseCase.ExecuteAsync(Arg.Any<MoveSpaceCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SpaceView>(
                new ArgumentException("Parent space cannot be the space itself or one of its descendants.")));
        var controller = CreateController(moveSpaceUseCase, Substitute.For<IDeleteSpaceUseCase>());

        var result = await controller.MoveSpace(Guid.NewGuid(), Guid.NewGuid(), new MoveSpaceRequest(Guid.NewGuid()),
            CancellationToken.None);

        var response = Assert.IsType<BadRequestObjectResult>(result.Result);
        var problem = Assert.IsType<ProblemDetails>(response.Value);
        Assert.Equal("Invalid space move.", problem.Title);
    }

    [Fact]
    public async Task DeleteSpace_WhenSpaceIsNotEmpty_ReturnsConflictWithStableProblemDetail()
    {
        var deleteSpaceUseCase = Substitute.For<IDeleteSpaceUseCase>();
        deleteSpaceUseCase.ExecuteAsync(Arg.Any<DeleteSpaceCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new SpaceNotEmptyException()));
        var controller = CreateController(Substitute.For<IMoveSpaceUseCase>(), deleteSpaceUseCase);

        var result = await controller.DeleteSpace(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        var response = Assert.IsType<ConflictObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(response.Value);
        Assert.Equal("Space is not empty.", problem.Title);
        Assert.Equal(SpaceNotEmptyException.Detail, problem.Detail);
    }

    [Fact]
    public async Task MoveSpace_WhenValid_PreservesSuccessfulResponseEnvelope()
    {
        var householdId = Guid.NewGuid();
        var space = new SpaceView(Guid.NewGuid(), householdId, null, "Pantry", null, null, null);
        var moveSpaceUseCase = Substitute.For<IMoveSpaceUseCase>();
        moveSpaceUseCase.ExecuteAsync(Arg.Any<MoveSpaceCommand>(), Arg.Any<CancellationToken>())
            .Returns(space);
        var controller = CreateController(moveSpaceUseCase, Substitute.For<IDeleteSpaceUseCase>());

        var result = await controller.MoveSpace(householdId, space.Id, new MoveSpaceRequest(null), CancellationToken.None);

        var response = Assert.IsType<OkObjectResult>(result.Result);
        var envelope = Assert.IsType<ApiResponse<SpaceView>>(response.Value);
        Assert.Equal(space, envelope.Data);
    }

    private static SpacesController CreateController(IMoveSpaceUseCase moveSpaceUseCase, IDeleteSpaceUseCase deleteSpaceUseCase)
    {
        var actorAccessor = Substitute.For<IActorAccessor>();
        actorAccessor.DomuActor.Returns(new DomuActor(Guid.NewGuid(), DomuActorType.Zitadel));

        return new SpacesController(
            actorAccessor,
            Substitute.For<ICreateSpaceUseCase>(),
            Substitute.For<IGetSpaceUseCase>(),
            Substitute.For<IGetSpacesPageUseCase>(),
            Substitute.For<IUpdateSpaceUseCase>(),
            moveSpaceUseCase,
            deleteSpaceUseCase);
    }
}
