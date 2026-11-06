using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Authentication;
using SAPSec.Core.FeatureFlags;
using SAPSec.Core.Time;
using SAPSec.Data.Dto;
using SAPSec.Data.Repositories;
using SAPSec.Web.Authorization;
using SAPSec.Web.Constants;
using SAPSec.Web.Controllers;
using SAPSec.Web.Services;
using System.Security.Claims;

namespace SAPSec.Web.Tests.Deprecated.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<ILogger<UserController>> _logger = new();
    private readonly Mock<IFeatureFlagService> _featureFlagService = new();
    private readonly Mock<IDfeUserIdentifier> _dfeUserIdentifier = new();
    private readonly UserController _controller;

    public UserControllerTests()
    {
        var establishmentRepository = new Mock<IEstablishmentRepository>();
        establishmentRepository.Setup(x => x.GetEstablishmentAsync(It.IsAny<string>()))
            .ReturnsAsync((string urn) => new Establishment
            {
                URN = urn,
                PhaseOfEducationName = "Secondary",
                EstablishmentStatusId = "1"
            });
        var schoolUserAccessService = new SchoolUserAccessService(
            _userService.Object, _featureFlagService.Object,
            establishmentRepository.Object, Mock.Of<ICurrentTimeProvider>());
        _controller = new UserController(_userService.Object, _featureFlagService.Object,
            _dfeUserIdentifier.Object, schoolUserAccessService, _logger.Object);
        _featureFlagService.Setup(x => x.IsEnabledAsync(Flags.EnableFindASchool)).ReturnsAsync(false);
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
            Category = new Category { Id = "001", Name = "Establishment" },
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
            Category = new Category { Id = "001", Name = "Establishment" },
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
            Category = new Category { Id = "001", Name = "Establishment" },
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
    public async Task Index_WhenDfeUserWhoIsNotASchool_RedirectsToFindASchool()
    {
        SetupUserWithOrganisation(OtherOrganisation());
        _dfeUserIdentifier.Setup(x => x.IsDfeUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(true);

        var result = await _controller.Index();

        var redirect = result.Should().BeOfType<RedirectResult>().Subject;
        redirect.Url.Should().Be(Routes.FindASchool());
    }

    [Fact]
    public async Task Index_WhenNonDfeUserWhoIsNotASchool_ReturnsForbidden()
    {
        SetupUserWithOrganisation(OtherOrganisation());

        var result = await _controller.Index();

        result.Should().BeOfType<ForbidResult>();
    }

    private static Organisation OtherOrganisation() => new()
    {
        Id = "org-4",
        Name = "Department for Education",
        Category = new Category { Id = "011", Name = "Government" }
    };

    private void SetupUserWithOrganisation(Organisation organisation)
    {
        _userService.Setup(x => x.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new User { Sub = "user-123", Organisations = [organisation] });
        _userService.Setup(x => x.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(organisation);
    }
}
