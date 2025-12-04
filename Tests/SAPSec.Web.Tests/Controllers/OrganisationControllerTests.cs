using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Web.Controllers;
using System.Security.Claims;

namespace SAPSec.Web.Tests.Controllers;

public class OrganisationControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IDsiClient> _mockApiService;
    private readonly Mock<ILogger<OrganisationController>> _mockLogger;
    private readonly Mock<IUrlHelper> _mockUrlHelper;
    private readonly OrganisationController _controller;

    public OrganisationControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockApiService = new Mock<IDsiClient>();
        _mockLogger = new Mock<ILogger<OrganisationController>>();
        _mockUrlHelper = new Mock<IUrlHelper>();

        _controller = new OrganisationController(
            _mockUserService.Object,
            _mockApiService.Object,
            _mockLogger.Object);

        SetupControllerContext();
    }

    private void SetupControllerContext()
    {
        var httpContext = new DefaultHttpContext();
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        _mockUrlHelper.Setup(u => u.IsLocalUrl(It.IsAny<string>())).Returns(true);
        _mockUrlHelper.Setup(u => u.IsLocalUrl(null)).Returns(false);
        _mockUrlHelper.Setup(u => u.IsLocalUrl(string.Empty)).Returns(false);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        _controller.Url = _mockUrlHelper.Object;
        _controller.TempData = tempData;
    }

    private static User CreateTestUser(string userId = "test-user-id", List<Organisation>? organisations = null)
    {
        return new User
        {
            Name = userId,
            Sub = userId,
            Email = "test@example.com",
            Organisations = organisations ?? new List<Organisation>
            {
                new() { Id = "org-1", Name = "Organisation One" },
                new() { Id = "org-2", Name = "Organisation Two" }
            }
        };
    }

    private static Organisation CreateTestOrganisation(string id = "org-1", string name = "Test Organisation")
    {
        return new Organisation
        {
            Id = id,
            Name = name
        };
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullUserService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new OrganisationController(null!, _mockApiService.Object, _mockLogger.Object);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("userService");
    }

    [Fact]
    public void Constructor_WithNullApiService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new OrganisationController(_mockUserService.Object, null!, _mockLogger.Object);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("apiService");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new OrganisationController(_mockUserService.Object, _mockApiService.Object, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Act
        var controller = new OrganisationController(
            _mockUserService.Object,
            _mockApiService.Object,
            _mockLogger.Object);

        // Assert
        controller.Should().NotBeNull();
    }

    #endregion

    #region Details Tests

    [Fact]
    public async Task Details_WhenOrganisationExists_ReturnsViewWithOrganisation()
    {
        // Arrange
        var organisation = CreateTestOrganisation();
        _mockUserService.Setup(s => s.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(organisation);

        // Act
        var result = await _controller.Details();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.Model.Should().Be(organisation);
    }

    [Fact]
    public async Task Details_WhenOrganisationIsNull_RedirectsToError()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((Organisation?)null);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        // Act
        var result = await _controller.Details();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Error");
        redirectResult.ControllerName.Should().Be("Home");
    }

    [Fact]
    public async Task Details_WhenOrganisationIsNull_LogsWarning()
    {
        // Arrange
        var userId = "user-123";
        _mockUserService.Setup(s => s.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((Organisation?)null);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

        // Act
        await _controller.Details();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No organisation found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Details_WhenOrganisationIsNull_LogsUserId()
    {
        // Arrange
        var userId = "specific-user-id";
        _mockUserService.Setup(s => s.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((Organisation?)null);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

        // Act
        await _controller.Details();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(userId)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Details_CallsGetCurrentOrganisationAsync()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(CreateTestOrganisation());

        // Act
        await _controller.Details();

        // Assert
        _mockUserService.Verify(s => s.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
    }

    #endregion

    #region Switch GET Tests

    [Fact]
    public async Task Switch_Get_WhenUserHasOrganisations_ReturnsViewWithUser()
    {
        // Arrange
        var user = CreateTestUser();
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Switch();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.Model.Should().Be(user);
    }

    [Fact]
    public async Task Switch_Get_WhenUserIsNull_RedirectsToError()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.Switch();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Error");
        redirectResult.ControllerName.Should().Be("Home");
    }

    [Fact]
    public async Task Switch_Get_WhenUserHasNoOrganisations_RedirectsToError()
    {
        // Arrange
        var user = CreateTestUser(organisations: new List<Organisation>());
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Switch();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Error");
        redirectResult.ControllerName.Should().Be("Home");
    }

    [Fact]
    public async Task Switch_Get_WhenUserHasEmptyOrganisationsList_RedirectsToError()
    {
        // Arrange
        var user = new User
        {
            Name = "test-user",
            Organisations = new List<Organisation>()
        };
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Switch();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Switch_Get_WhenUserHasMultipleOrganisations_ReturnsViewWithAllOrganisations()
    {
        // Arrange
        var user = CreateTestUser(organisations: new List<Organisation>
        {
            new() { Id = "org-1", Name = "Org 1" },
            new() { Id = "org-2", Name = "Org 2" },
            new() { Id = "org-3", Name = "Org 3" }
        });
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Switch();

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        var model = viewResult!.Model as User;
        model!.Organisations.Should().HaveCount(3);
    }

    #endregion

    #region Switch POST Tests

    [Fact]
    public async Task Switch_Post_WithEmptyOrganisationId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Switch("", null);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Organisation ID is required");
    }

    [Fact]
    public async Task Switch_Post_WithNullOrganisationId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Switch(null!, null);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Organisation ID is required");
    }

    [Fact]
    public async Task Switch_Post_WhenUserIsNull_RedirectsToError()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.Switch("org-123", null);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Error");
        redirectResult.ControllerName.Should().Be("Home");
    }

    [Fact]
    public async Task Switch_Post_WhenOrganisationNotInUserList_ReturnsBadRequest()
    {
        // Arrange
        var user = CreateTestUser(organisations: new List<Organisation>
        {
            new() { Id = "org-1", Name = "Org 1" }
        });
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Switch("invalid-org-id", null);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Invalid organisation");
    }

    [Fact]
    public async Task Switch_Post_WhenOrganisationNotInUserList_LogsWarning()
    {
        // Arrange
        var user = CreateTestUser();
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        await _controller.Switch("invalid-org-id", null);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("attempted to switch to invalid organisation")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Switch_Post_WhenOrganisationNotInUserList_LogsOrganisationId()
    {
        // Arrange
        var invalidOrgId = "invalid-org-xyz";
        var user = CreateTestUser();
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        await _controller.Switch(invalidOrgId, null);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(invalidOrgId)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Switch_Post_WhenValidOrganisation_CallsSetCurrentOrganisationAsync()
    {
        // Arrange
        var organisationId = "org-1";
        var user = CreateTestUser();
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        await _controller.Switch(organisationId, null);

        // Assert
        _mockUserService.Verify(
            s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), organisationId),
            Times.Once);
    }

    [Fact]
    public async Task Switch_Post_WhenValidOrganisation_LogsInformation()
    {
        // Arrange
        var user = CreateTestUser();
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        await _controller.Switch("org-1", null);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("switched to organisation")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Switch_Post_WhenValidOrganisation_WithLocalReturnUrl_RedirectsToReturnUrl()
    {
        // Arrange
        var returnUrl = "/dashboard";
        var user = CreateTestUser();
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockUrlHelper.Setup(u => u.IsLocalUrl(returnUrl)).Returns(true);

        // Act
        var result = await _controller.Switch("org-1", returnUrl);

        // Assert
        result.Should().BeOfType<RedirectResult>();
        var redirectResult = result as RedirectResult;
        redirectResult!.Url.Should().Be(returnUrl);
    }

    [Fact]
    public async Task Switch_Post_WhenValidOrganisation_WithNullReturnUrl_RedirectsToDetails()
    {
        // Arrange
        var user = CreateTestUser();
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Switch("org-1", null);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Details");
    }

    [Fact]
    public async Task Switch_Post_WhenValidOrganisation_WithEmptyReturnUrl_RedirectsToDetails()
    {
        // Arrange
        var user = CreateTestUser();
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Switch("org-1", "");

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Details");
    }

    [Fact]
    public async Task Switch_Post_WhenValidOrganisation_WithExternalReturnUrl_RedirectsToDetails()
    {
        // Arrange
        var externalUrl = "https://malicious-site.com";
        var user = CreateTestUser();
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockUrlHelper.Setup(u => u.IsLocalUrl(externalUrl)).Returns(false);

        // Act
        var result = await _controller.Switch("org-1", externalUrl);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Details");
    }

    [Fact]
    public async Task Switch_Post_LogsUserSub()
    {
        // Arrange
        var userSub = "user-sub-123";
        var user = CreateTestUser(userId: userSub);
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        await _controller.Switch("org-1", null);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(userSub)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region GetCurrent API Tests

    [Fact]
    public async Task GetCurrent_WhenOrganisationExists_ReturnsJsonResult()
    {
        // Arrange
        var organisation = CreateTestOrganisation();
        _mockUserService.Setup(s => s.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(organisation);

        // Act
        var result = await _controller.GetCurrent();

        // Assert
        result.Should().BeOfType<JsonResult>();
        var jsonResult = result as JsonResult;
        jsonResult!.Value.Should().Be(organisation);
    }

    [Fact]
    public async Task GetCurrent_WhenOrganisationIsNull_ReturnsNotFound()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((Organisation?)null);

        // Act
        var result = await _controller.GetCurrent();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetCurrent_CallsGetCurrentOrganisationAsync()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(CreateTestOrganisation());

        // Act
        await _controller.GetCurrent();

        // Assert
        _mockUserService.Verify(s => s.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrent_ReturnsOrganisationWithCorrectProperties()
    {
        // Arrange
        var organisation = new Organisation
        {
            Id = "org-specific",
            Name = "Specific Organisation"
        };
        _mockUserService.Setup(s => s.GetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(organisation);

        // Act
        var result = await _controller.GetCurrent();

        // Assert
        var jsonResult = result as JsonResult;
        var returnedOrg = jsonResult!.Value as Organisation;
        returnedOrg!.Id.Should().Be("org-specific");
        returnedOrg.Name.Should().Be("Specific Organisation");
    }

    #endregion

    #region GetOrganisation API Tests

    [Fact]
    public async Task GetOrganisation_WithEmptyId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetOrganisation("");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Organisation ID is required");
    }

    [Fact]
    public async Task GetOrganisation_WithNullId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetOrganisation(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Organisation ID is required");
    }

    [Fact]
    public async Task GetOrganisation_WhenOrganisationExists_ReturnsJsonResult()
    {
        // Arrange
        var organisationId = "org-123";
        var organisation = CreateTestOrganisation(id: organisationId);
        _mockApiService.Setup(s => s.GetOrganisationAsync(organisationId))
            .ReturnsAsync(organisation);

        // Act
        var result = await _controller.GetOrganisation(organisationId);

        // Assert
        result.Should().BeOfType<JsonResult>();
        var jsonResult = result as JsonResult;
        jsonResult!.Value.Should().Be(organisation);
    }

    [Fact]
    public async Task GetOrganisation_WhenOrganisationNotFound_ReturnsNotFound()
    {
        // Arrange
        var organisationId = "non-existent-org";
        _mockApiService.Setup(s => s.GetOrganisationAsync(organisationId))
            .ReturnsAsync((Organisation?)null);

        // Act
        var result = await _controller.GetOrganisation(organisationId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetOrganisation_CallsApiServiceWithCorrectId()
    {
        // Arrange
        var organisationId = "specific-org-id";
        _mockApiService.Setup(s => s.GetOrganisationAsync(organisationId))
            .ReturnsAsync(CreateTestOrganisation());

        // Act
        await _controller.GetOrganisation(organisationId);

        // Assert
        _mockApiService.Verify(s => s.GetOrganisationAsync(organisationId), Times.Once);
    }

    [Fact]
    public async Task GetOrganisation_ReturnsOrganisationWithCorrectProperties()
    {
        // Arrange
        var organisation = new Organisation
        {
            Id = "org-api",
            Name = "API Organisation"
        };
        _mockApiService.Setup(s => s.GetOrganisationAsync("org-api"))
            .ReturnsAsync(organisation);

        // Act
        var result = await _controller.GetOrganisation("org-api");

        // Assert
        var jsonResult = result as JsonResult;
        var returnedOrg = jsonResult!.Value as Organisation;
        returnedOrg!.Id.Should().Be("org-api");
        returnedOrg.Name.Should().Be("API Organisation");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Switch_Post_WithSpecialCharactersInOrganisationId_ProcessesCorrectly()
    {
        // Arrange
        var specialOrgId = "org-!@#$%";
        var user = CreateTestUser(organisations: new List<Organisation>
        {
            new() { Id = specialOrgId, Name = "Special Org" }
        });
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Switch(specialOrgId, null);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        _mockUserService.Verify(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), specialOrgId), Times.Once);
    }

    [Fact]
    public async Task GetOrganisation_WithSpecialCharactersInId_CallsApiService()
    {
        // Arrange
        var specialOrgId = "org-!@#$%^&*()";
        _mockApiService.Setup(s => s.GetOrganisationAsync(specialOrgId))
            .ReturnsAsync(CreateTestOrganisation(id: specialOrgId));

        // Act
        var result = await _controller.GetOrganisation(specialOrgId);

        // Assert
        result.Should().BeOfType<JsonResult>();
        _mockApiService.Verify(s => s.GetOrganisationAsync(specialOrgId), Times.Once);
    }

    [Fact]
    public async Task Switch_Post_WithVeryLongOrganisationId_ProcessesCorrectly()
    {
        // Arrange
        var longOrgId = new string('a', 1000);
        var user = CreateTestUser(organisations: new List<Organisation>
        {
            new() { Id = longOrgId, Name = "Long ID Org" }
        });
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Switch(longOrgId, null);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Theory]
    [InlineData("/local/path")]
    [InlineData("/dashboard")]
    [InlineData("/organisation/details")]
    public async Task Switch_Post_WithVariousLocalUrls_RedirectsCorrectly(string returnUrl)
    {
        // Arrange
        var user = CreateTestUser();
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockUrlHelper.Setup(u => u.IsLocalUrl(returnUrl)).Returns(true);

        // Act
        var result = await _controller.Switch("org-1", returnUrl);

        // Assert
        result.Should().BeOfType<RedirectResult>();
        var redirectResult = result as RedirectResult;
        redirectResult!.Url.Should().Be(returnUrl);
    }

    [Theory]
    [InlineData("https://external.com")]
    [InlineData("//evil.com")]
    [InlineData("http://malicious.org")]
    public async Task Switch_Post_WithExternalUrls_RedirectsToDetails(string externalUrl)
    {
        // Arrange
        var user = CreateTestUser();
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _mockUrlHelper.Setup(u => u.IsLocalUrl(externalUrl)).Returns(false);

        // Act
        var result = await _controller.Switch("org-1", externalUrl);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Details");
    }

    #endregion
}