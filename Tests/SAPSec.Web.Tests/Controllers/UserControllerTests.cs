using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Authentication;
using SAPSec.Core.Model;
using SAPSec.Web.Controllers;
using System.Security.Claims;

namespace SAPSec.Web.Tests.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<ILogger<UserController>> _logger = new();
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _controller = new UserController(_userService.Object, _logger.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, "user-123")],
                    "TestAuth"))
            }
        };
    }

    [Fact]
    public async Task Index_WhenCurrentOrganisationIsPrimary_RedirectsToPrimarySchoolRoute()
    {
        var user = new User
        {
            Sub = "user-123",
            Organisations = [new Organisation { Id = "org-1", Urn = "123456" }]
        };
        var organisation = new Organisation
        {
            Id = "org-1",
            Name = "Primary School",
            Category = new Category { Name = "Establishment" },
            Urn = "123456",
            PhaseOfEducation = new PhaseOfEducation { Name = "Primary" }
        };

        _userService.Setup(x => x.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _userService.Setup(x => x.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(organisation);

        var result = await _controller.Index();

        var redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be("/school/primary/123456");
    }

    [Fact]
    public async Task Index_WhenCurrentOrganisationIsSecondary_RedirectsToSecondarySchoolRoute()
    {
        var user = new User
        {
            Sub = "user-123",
            Organisations = [new Organisation { Id = "org-2", Urn = "654321" }]
        };
        var organisation = new Organisation
        {
            Id = "org-2",
            Name = "Secondary School",
            Category = new Category { Name = "Establishment" },
            Urn = "654321",
            PhaseOfEducation = new PhaseOfEducation { Name = "Secondary" }
        };

        _userService.Setup(x => x.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _userService.Setup(x => x.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(organisation);

        var result = await _controller.Index();

        var redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be("/school/654321");
    }
}
