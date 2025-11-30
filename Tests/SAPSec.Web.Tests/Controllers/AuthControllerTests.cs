using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
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
public class AuthControllerTests
{
    private readonly Mock<IDsiUserService> _mockUserService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;
    private readonly Mock<IUrlHelper> _mockUrlHelper;
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<ISession> _mockSession;

    public AuthControllerTests()
    {
        _mockUserService = new Mock<IDsiUserService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockUrlHelper = new Mock<IUrlHelper>();
        _mockSession = new Mock<ISession>();

        _controller = new AuthController(_mockUserService.Object, _mockLogger.Object);

        SetupControllerContext();
    }

    private void SetupControllerContext()
    {
        var httpContext = new DefaultHttpContext();

        // Setup service provider with authentication service
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(sp => sp.GetService(typeof(IAuthenticationService)))
            .Returns(_mockAuthService.Object);
        httpContext.RequestServices = serviceProvider.Object;

        // Setup session
        httpContext.Session = _mockSession.Object;

        // Setup URL helper
        _mockUrlHelper.Setup(u => u.IsLocalUrl(It.IsAny<string>())).Returns(true);
        _mockUrlHelper.Setup(u => u.IsLocalUrl(null)).Returns(false);
        _mockUrlHelper.Setup(u => u.IsLocalUrl(string.Empty)).Returns(false);

        // Setup TempData
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        _controller.Url = _mockUrlHelper.Object;
        _controller.TempData = tempData;
    }

    private ClaimsPrincipal CreateAuthenticatedUser(string userId = "test-user-id")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullUserService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new AuthController(null!, _mockLogger.Object);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("userService");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var action = () => new AuthController(_mockUserService.Object, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Act
        var controller = new AuthController(_mockUserService.Object, _mockLogger.Object);

        // Assert
        controller.Should().NotBeNull();
    }

    #endregion

    #region SignIn Tests

    [Fact]
    public void SignIn_WhenUserAlreadyAuthenticated_WithLocalReturnUrl_RedirectsToReturnUrl()
    {
        // Arrange
        var returnUrl = "/dashboard";
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(true);
        _mockUrlHelper.Setup(u => u.IsLocalUrl(returnUrl)).Returns(true);

        // Act
        var result = _controller.SignIn(returnUrl);

        // Assert
        result.Should().BeOfType<RedirectResult>();
        var redirectResult = result as RedirectResult;
        redirectResult!.Url.Should().Be(returnUrl);
    }

    [Fact]
    public void SignIn_WhenUserAlreadyAuthenticated_WithNullReturnUrl_RedirectsToHome()
    {
        // Arrange
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(true);

        // Act
        var result = _controller.SignIn(null);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Index");
        redirectResult.ControllerName.Should().Be("Home");
    }

    [Fact]
    public void SignIn_WhenUserAlreadyAuthenticated_WithEmptyReturnUrl_RedirectsToHome()
    {
        // Arrange
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(true);

        // Act
        var result = _controller.SignIn(string.Empty);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Index");
        redirectResult.ControllerName.Should().Be("Home");
    }

    [Fact]
    public void SignIn_WhenUserAlreadyAuthenticated_WithExternalUrl_RedirectsToHome()
    {
        // Arrange
        var externalUrl = "https://malicious-site.com";
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(true);
        _mockUrlHelper.Setup(u => u.IsLocalUrl(externalUrl)).Returns(false);

        // Act
        var result = _controller.SignIn(externalUrl);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Index");
        redirectResult.ControllerName.Should().Be("Home");
    }

    [Fact]
    public void SignIn_WhenUserNotAuthenticated_ReturnsChallengeResult()
    {
        // Arrange
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(false);

        // Act
        var result = _controller.SignIn("/returnUrl");

        // Assert
        result.Should().BeOfType<ChallengeResult>();
        var challengeResult = result as ChallengeResult;
        challengeResult!.AuthenticationSchemes.Should().Contain(OpenIdConnectDefaults.AuthenticationScheme);
    }

    [Fact]
    public void SignIn_WhenUserNotAuthenticated_WithNullReturnUrl_SetsDefaultRedirectUri()
    {
        // Arrange
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(false);

        // Act
        var result = _controller.SignIn(null);

        // Assert
        result.Should().BeOfType<ChallengeResult>();
        var challengeResult = result as ChallengeResult;
        challengeResult!.Properties.Should().NotBeNull();
        challengeResult.Properties!.RedirectUri.Should().Be("/Search");
    }

    [Fact]
    public void SignIn_WhenUserNotAuthenticated_WithReturnUrl_SetsCorrectRedirectUri()
    {
        // Arrange
        var returnUrl = "/custom/path";
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(false);

        // Act
        var result = _controller.SignIn(returnUrl);

        // Assert
        result.Should().BeOfType<ChallengeResult>();
        var challengeResult = result as ChallengeResult;
        challengeResult!.Properties!.RedirectUri.Should().Be(returnUrl);
    }

    [Fact]
    public void SignIn_WhenUserNotAuthenticated_UsesOpenIdConnectScheme()
    {
        // Arrange
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(false);

        // Act
        var result = _controller.SignIn(null);

        // Assert
        result.Should().BeOfType<ChallengeResult>();
        var challengeResult = result as ChallengeResult;
        challengeResult!.AuthenticationSchemes.Should().HaveCount(1);
        challengeResult.AuthenticationSchemes.Should().Contain(OpenIdConnectDefaults.AuthenticationScheme);
    }

    #endregion

    #region SelectOrganisation GET Tests

    [Fact]
    public async Task SelectOrganisation_Get_WithNullUser_RedirectsToError()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((DsiUser?)null);

        // Act
        var result = await _controller.SelectOrganisation(null);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Error");
        redirectResult.ControllerName.Should().Be("Home");
    }

    [Fact]
    public async Task SelectOrganisation_Get_WithUserWithNoOrganisations_RedirectsToError()
    {
        // Arrange
        var user = new DsiUser
        {
            Name = "test-user",
            Organisations = new List<DsiOrganisation>()
        };
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.SelectOrganisation(null);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Error");
        redirectResult.ControllerName.Should().Be("Home");
    }

    [Fact]
    public async Task SelectOrganisation_Get_WithUserWithEmptyOrganisationsList_RedirectsToError()
    {
        // Arrange
        var user = new DsiUser
        {
            Name = "test-user",
            Organisations = new List<DsiOrganisation>()
        };
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.SelectOrganisation("/return");

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Error");
    }

    [Fact]
    public async Task SelectOrganisation_Get_WithValidUser_ReturnsView()
    {
        // Arrange
        var user = new DsiUser
        {
            Name = "test-user",
            Organisations = new List<DsiOrganisation>
            {
                new() { Id = "org1", Name = "Organisation 1" }
            }
        };
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.SelectOrganisation("/return");

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.Model.Should().Be(user);
    }

    [Fact]
    public async Task SelectOrganisation_Get_WithValidUser_SetsReturnUrlInViewBag()
    {
        // Arrange
        var user = new DsiUser
        {
            Name = "test-user",
            Organisations = new List<DsiOrganisation>
            {
                new() { Id = "org1", Name = "Organisation 1" }
            }
        };
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.SelectOrganisation("/custom-return");

        // Assert
        result.Should().BeOfType<ViewResult>();
        ((string)_controller.ViewBag.ReturnUrl).Should().Be("/custom-return");
    }

    [Fact]
    public async Task SelectOrganisation_Get_WithMultipleOrganisations_ReturnsViewWithAllOrganisations()
    {
        // Arrange
        var user = new DsiUser
        {
            Name = "test-user",
            Organisations = new List<DsiOrganisation>
            {
                new() { Id = "org1", Name = "Organisation 1" },
                new() { Id = "org2", Name = "Organisation 2" },
                new() { Id = "org3", Name = "Organisation 3" }
            }
        };
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.SelectOrganisation(null);

        // Assert
        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        var model = viewResult!.Model as DsiUser;
        model!.Organisations.Should().HaveCount(3);
    }

    [Fact]
    public async Task SelectOrganisation_Get_WithNullReturnUrl_SetsNullInViewBag()
    {
        // Arrange
        var user = new DsiUser
        {
            Name = "test-user",
            Organisations = new List<DsiOrganisation>
            {
                new() { Id = "org1", Name = "Organisation 1" }
            }
        };
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.SelectOrganisation(null);

        // Assert
        result.Should().BeOfType<ViewResult>();
        ((string?)_controller.ViewBag.ReturnUrl).Should().BeNull();
    }

    #endregion

    #region SelectOrganisation POST Tests

    [Fact]
    public async Task SelectOrganisation_Post_WithEmptyOrganisationId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.SelectOrganisation("", "/return");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Organisation ID is required");
    }

    [Fact]
    public async Task SelectOrganisation_Post_WithNullOrganisationId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.SelectOrganisation(null!, "/return");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Organisation ID is required");
    }

    [Fact]
    public async Task SelectOrganisation_Post_WhenSetOrganisationFails_RedirectsToError()
    {
        // Arrange
        _mockUserService.Setup(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        // Act
        var result = await _controller.SelectOrganisation("org-123", "/return");

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Error");
        redirectResult.ControllerName.Should().Be("Home");
    }

    [Fact]
    public async Task SelectOrganisation_Post_WhenSetOrganisationFails_LogsWarning()
    {
        // Arrange
        _mockUserService.Setup(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        // Act
        await _controller.SelectOrganisation("org-123", "/return");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to set organisation")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SelectOrganisation_Post_WhenSetOrganisationFails_LogsOrganisationId()
    {
        // Arrange
        var organisationId = "org-456";
        _mockUserService.Setup(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        // Act
        await _controller.SelectOrganisation(organisationId, "/return");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(organisationId)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SelectOrganisation_Post_WhenSuccessful_RedirectsToReturnUrl()
    {
        // Arrange
        var returnUrl = "/dashboard";
        _mockUserService.Setup(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");
        _mockUrlHelper.Setup(u => u.IsLocalUrl(returnUrl)).Returns(true);

        // Act
        var result = await _controller.SelectOrganisation("org-123", returnUrl);

        // Assert
        result.Should().BeOfType<RedirectResult>();
        var redirectResult = result as RedirectResult;
        redirectResult!.Url.Should().Be(returnUrl);
    }

    [Fact]
    public async Task SelectOrganisation_Post_WhenSuccessful_WithNullReturnUrl_RedirectsToHome()
    {
        // Arrange
        _mockUserService.Setup(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        // Act
        var result = await _controller.SelectOrganisation("org-123", null);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Index");
        redirectResult.ControllerName.Should().Be("Home");
    }

    [Fact]
    public async Task SelectOrganisation_Post_WhenSuccessful_WithExternalReturnUrl_RedirectsToHome()
    {
        // Arrange
        var externalUrl = "https://evil.com";
        _mockUserService.Setup(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");
        _mockUrlHelper.Setup(u => u.IsLocalUrl(externalUrl)).Returns(false);

        // Act
        var result = await _controller.SelectOrganisation("org-123", externalUrl);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Index");
        redirectResult.ControllerName.Should().Be("Home");
    }

    [Fact]
    public async Task SelectOrganisation_Post_WhenSuccessful_LogsInformation()
    {
        // Arrange
        _mockUserService.Setup(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        // Act
        await _controller.SelectOrganisation("org-123", "/return");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("selected organisation")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SelectOrganisation_Post_WhenSuccessful_LogsUserId()
    {
        // Arrange
        var userId = "user-789";
        _mockUserService.Setup(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

        // Act
        await _controller.SelectOrganisation("org-123", "/return");

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(userId)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SelectOrganisation_Post_CallsSetCurrentOrganisationWithCorrectParameters()
    {
        // Arrange
        var organisationId = "org-test-123";
        _mockUserService.Setup(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        // Act
        await _controller.SelectOrganisation(organisationId, "/return");

        // Assert
        _mockUserService.Verify(
            s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), organisationId),
            Times.Once);
    }

    #endregion

    #region SignOutCallback Tests

    [Fact]
    public async Task SignOutCallback_ClearsSession()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        // Act
        await _controller.SignOutCallback();

        // Assert
        _mockSession.Verify(s => s.Clear(), Times.Once);
    }

    [Fact]
    public async Task SignOutCallback_SignsOutOfCookieScheme()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        // Act
        await _controller.SignOutCallback();

        // Assert
        _mockAuthService.Verify(
            a => a.SignOutAsync(
                It.IsAny<HttpContext>(),
                CookieAuthenticationDefaults.AuthenticationScheme,
                It.IsAny<AuthenticationProperties>()),
            Times.Once);
    }

    [Fact]
    public async Task SignOutCallback_SignsOutOfOpenIdConnectScheme()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        // Act
        await _controller.SignOutCallback();

        // Assert
        _mockAuthService.Verify(
            a => a.SignOutAsync(
                It.IsAny<HttpContext>(),
                OpenIdConnectDefaults.AuthenticationScheme,
                It.IsAny<AuthenticationProperties>()),
            Times.Once);
    }

    [Fact]
    public async Task SignOutCallback_RedirectsToHomeIndex()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        // Act
        var result = await _controller.SignOutCallback();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Index");
        redirectResult.ControllerName.Should().Be("Home");
    }

    [Fact]
    public async Task SignOutCallback_LogsSignOutInformation()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        // Act
        await _controller.SignOutCallback();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("signing out")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SignOutCallback_LogsUserId()
    {
        // Arrange
        var userId = "user-to-logout";
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

        // Act
        await _controller.SignOutCallback();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(userId)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SignOutCallback_ClearsSessionBeforeSignOut()
    {
        // Arrange
        var callOrder = new List<string>();
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");
        _mockSession.Setup(s => s.Clear()).Callback(() => callOrder.Add("SessionClear"));
        _mockAuthService.Setup(a => a.SignOutAsync(
            It.IsAny<HttpContext>(),
            It.IsAny<string>(),
            It.IsAny<AuthenticationProperties>()))
            .Callback(() => callOrder.Add("SignOut"))
            .Returns(Task.CompletedTask);

        // Act
        await _controller.SignOutCallback();

        // Assert
        callOrder.First().Should().Be("SessionClear");
    }

    #endregion

    #region AccessDenied Tests

    [Fact]
    public void AccessDenied_ReturnsView()
    {
        // Act
        var result = _controller.AccessDenied();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void AccessDenied_ReturnsDefaultView()
    {
        // Act
        var result = _controller.AccessDenied() as ViewResult;

        // Assert
        result!.ViewName.Should().BeNull(); // Default view name
    }

    [Fact]
    public void AccessDenied_ReturnsViewWithNoModel()
    {
        // Act
        var result = _controller.AccessDenied() as ViewResult;

        // Assert
        result!.Model.Should().BeNull();
    }

    #endregion

    #region SignedOut Tests

    [Fact]
    public void SignedOut_ReturnsView()
    {
        // Act
        var result = _controller.SignedOut();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void SignedOut_ReturnsDefaultView()
    {
        // Act
        var result = _controller.SignedOut() as ViewResult;

        // Assert
        result!.ViewName.Should().BeNull(); // Default view name
    }

    [Fact]
    public void SignedOut_ReturnsViewWithNoModel()
    {
        // Act
        var result = _controller.SignedOut() as ViewResult;

        // Assert
        result!.Model.Should().BeNull();
    }

    #endregion

    #region RedirectToLocal Tests (Indirect via SignIn and SelectOrganisation)

    [Theory]
    [InlineData("/local/path")]
    [InlineData("/dashboard")]
    [InlineData("/school/search")]
    public void SignIn_AuthenticatedUser_WithLocalUrl_RedirectsToLocalUrl(string localUrl)
    {
        // Arrange
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(true);
        _mockUrlHelper.Setup(u => u.IsLocalUrl(localUrl)).Returns(true);

        // Act
        var result = _controller.SignIn(localUrl);

        // Assert
        result.Should().BeOfType<RedirectResult>();
        var redirectResult = result as RedirectResult;
        redirectResult!.Url.Should().Be(localUrl);
    }

    [Theory]
    [InlineData("https://external.com")]
    [InlineData("//evil.com")]
    [InlineData("http://malicious.org/path")]
    public void SignIn_AuthenticatedUser_WithExternalUrl_RedirectsToHome(string externalUrl)
    {
        // Arrange
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(true);
        _mockUrlHelper.Setup(u => u.IsLocalUrl(externalUrl)).Returns(false);

        // Act
        var result = _controller.SignIn(externalUrl);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Index");
        redirectResult.ControllerName.Should().Be("Home");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void SignIn_AuthenticatedUser_WithNullOrEmptyUrl_RedirectsToHome(string? url)
    {
        // Arrange
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(true);

        // Act
        var result = _controller.SignIn(url);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be("Index");
        redirectResult.ControllerName.Should().Be("Home");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task SelectOrganisation_Post_WithVeryLongOrganisationId_ProcessesCorrectly()
    {
        // Arrange
        var longOrgId = new string('a', 1000);
        _mockUserService.Setup(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        // Act
        var result = await _controller.SelectOrganisation(longOrgId, "/return");

        // Assert
        result.Should().BeOfType<RedirectResult>();
        _mockUserService.Verify(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), longOrgId), Times.Once);
    }

    [Fact]
    public async Task SelectOrganisation_Post_WithSpecialCharactersInOrganisationId_ProcessesCorrectly()
    {
        // Arrange
        var specialOrgId = "org-123-!@#$%^&*()";
        _mockUserService.Setup(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        // Act
        var result = await _controller.SelectOrganisation(specialOrgId, "/return");

        // Assert
        result.Should().BeOfType<RedirectResult>();
    }

    [Fact]
    public void SignIn_WithSpecialCharactersInReturnUrl_ProcessesCorrectly()
    {
        // Arrange
        var specialUrl = "/path?query=value&other=123";
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(false);

        // Act
        var result = _controller.SignIn(specialUrl);

        // Assert
        result.Should().BeOfType<ChallengeResult>();
        var challengeResult = result as ChallengeResult;
        challengeResult!.Properties!.RedirectUri.Should().Be(specialUrl);
    }

    #endregion
}