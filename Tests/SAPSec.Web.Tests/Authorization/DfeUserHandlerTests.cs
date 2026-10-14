using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using SAPSec.Core.Authentication;
using SAPSec.Web.Authorization;
using System.Security.Claims;

namespace SAPSec.Web.Tests.Deprecated.Authorization;

public class DfeUserHandlerTests
{
    private readonly Mock<IUserService> _userService = new();
    private readonly DfeUserRequirement _requirement = new();
    private readonly ClaimsPrincipal _principal = new(new ClaimsIdentity(
        [new Claim(ClaimTypes.NameIdentifier, "user-123")],
        "TestAuth"));

    [Fact]
    public async Task HandleAsync_WhenUserHasDfeRole_Succeeds()
    {
        _userService.Setup(x => x.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(true);
        _userService.Setup(x => x.HasRole(It.IsAny<ClaimsPrincipal>(), UserRoles.DfeUser)).Returns(true);

        var context = await HandleAsync();

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotHaveDfeRole_DoesNotSucceed()
    {
        _userService.Setup(x => x.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(true);
        _userService.Setup(x => x.HasRole(It.IsAny<ClaimsPrincipal>(), UserRoles.DfeUser)).Returns(false);

        var context = await HandleAsync();

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_DoesNotSucceed()
    {
        _userService.Setup(x => x.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(false);
        _userService.Setup(x => x.HasRole(It.IsAny<ClaimsPrincipal>(), UserRoles.DfeUser)).Returns(true);

        var context = await HandleAsync();

        context.HasSucceeded.Should().BeFalse();
    }

    private async Task<AuthorizationHandlerContext> HandleAsync()
    {
        var context = new AuthorizationHandlerContext([_requirement], _principal, null);

        await new DfeUserHandler(_userService.Object).HandleAsync(context);

        return context;
    }
}
