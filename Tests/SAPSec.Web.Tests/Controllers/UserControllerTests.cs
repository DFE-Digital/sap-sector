using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Authentication;
using SAPSec.Web.Authorization;
using SAPSec.Web.Constants;
using SAPSec.Web.Controllers;
using System.Security.Claims;

namespace SAPSec.Web.Tests.Deprecated.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IAuthorizationService> _authorizationService = new();
    private readonly Mock<ILogger<UserController>> _logger = new();
    private readonly UserController _controller;

    public UserControllerTests()
    {
        SetupTrustOrLocalAuthorityPolicyResult(AuthorizationResult.Failed());

        _controller = new UserController(_userService.Object, _authorizationService.Object, _logger.Object);
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
        redirect.Url.Should().Be(Routes.PrimarySchool("123456").Overview);
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
        redirect.Url.Should().Be(Routes.SecondarySchool("654321").Overview);
    }

    [Fact]
    public async Task Index_WhenCurrentOrganisationIsAllThrough_RedirectsToAllThroughSchoolRoute()
    {
        var user = new User
        {
            Sub = "user-123",
            Organisations = [new Organisation { Id = "org-3", Urn = "111111" }]
        };
        var organisation = new Organisation
        {
            Id = "org-3",
            Name = "All-through School",
            Category = new Category { Name = "Establishment" },
            Urn = "111111",
            PhaseOfEducation = new PhaseOfEducation { Name = "All-through" }
        };

        _userService.Setup(x => x.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _userService.Setup(x => x.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(organisation);

        var result = await _controller.Index();

        var redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be(Routes.AllThroughSchool("111111").Overview);
    }

    [Fact]
    public async Task Index_WhenUserMeetsTrustOrLocalAuthorityPolicy_RedirectsToYourSchools()
    {
        var organisation = new Organisation
        {
            Id = "org-4",
            Name = "Test Trust",
            Category = new Category { Id = OrganisationCategoryIds.MultiAcademyTrust, Name = "Multi-Academy Trust" },
            Uid = "5001"
        };

        _userService.Setup(x => x.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new User { Sub = "user-123", Organisations = [organisation] });
        _userService.Setup(x => x.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(organisation);
        SetupTrustOrLocalAuthorityPolicyResult(AuthorizationResult.Success());

        var result = await _controller.Index();

        var redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be(Routes.YourSchools);
    }

    [Fact]
    public async Task Index_WhenOrganisationIsNotEstablishmentAndFailsTrustOrLocalAuthorityPolicy_RedirectsToFindASchool()
    {
        var organisation = new Organisation
        {
            Id = "org-5",
            Name = "Other Organisation",
            Category = new Category { Id = "011", Name = "Government" }
        };

        _userService.Setup(x => x.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new User { Sub = "user-123", Organisations = [organisation] });
        _userService.Setup(x => x.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(organisation);

        var result = await _controller.Index();

        var redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be(Routes.FindASchool());
    }

    private void SetupTrustOrLocalAuthorityPolicyResult(AuthorizationResult result)
    {
        _authorizationService
            .Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object?>(),
                AuthorizationPolicies.TrustOrLocalAuthorityUser))
            .ReturnsAsync(result);
    }
}
