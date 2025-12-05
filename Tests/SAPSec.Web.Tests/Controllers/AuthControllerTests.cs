using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
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
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;
    private readonly Mock<IUrlHelper> _mockUrlHelper;
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<ISession> _mockSession;

    private static class ExpectedRoutes
    {
        public const string DefaultReturnUrl = "/school/search-for-a-school";
        public const string HomeAction = "Index";
        public const string HomeController = "Home";
        public const string ErrorAction = "StatusCodeError";
        public const string ErrorController = "Error";
    }

    public AuthControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
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

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(sp => sp.GetService(typeof(IAuthenticationService)))
            .Returns(_mockAuthService.Object);
        httpContext.RequestServices = serviceProvider.Object;

        httpContext.Session = _mockSession.Object;

        _mockUrlHelper.Setup(u => u.IsLocalUrl(It.IsAny<string>())).Returns(true);
        _mockUrlHelper.Setup(u => u.IsLocalUrl(null)).Returns(false);
        _mockUrlHelper.Setup(u => u.IsLocalUrl(string.Empty)).Returns(false);

        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        _controller.Url = _mockUrlHelper.Object;
        _controller.TempData = tempData;
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullUserService_ThrowsArgumentNullException()
    {
        var action = () => new AuthController(null!, _mockLogger.Object);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("userService");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        var action = () => new AuthController(_mockUserService.Object, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var controller = new AuthController(_mockUserService.Object, _mockLogger.Object);
        controller.Should().NotBeNull();
    }

    #endregion

    #region SignIn Tests

    [Fact]
    public void SignIn_WhenUserAlreadyAuthenticated_WithLocalReturnUrl_RedirectsToReturnUrl()
    {
        var returnUrl = "/dashboard";
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(true);
        _mockUrlHelper.Setup(u => u.IsLocalUrl(returnUrl)).Returns(true);

        var result = _controller.SignIn(returnUrl);

        result.Should().BeOfType<RedirectResult>();
        var redirectResult = result as RedirectResult;
        redirectResult!.Url.Should().Be(returnUrl);
    }

    [Fact]
    public void SignIn_WhenUserAlreadyAuthenticated_WithNullReturnUrl_RedirectsToHome()
    {
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(true);

        var result = _controller.SignIn(null);

        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be(ExpectedRoutes.HomeAction);
        redirectResult.ControllerName.Should().Be(ExpectedRoutes.HomeController);
    }

    [Fact]
    public void SignIn_WhenUserAlreadyAuthenticated_WithEmptyReturnUrl_RedirectsToHome()
    {
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(true);

        var result = _controller.SignIn(string.Empty);

        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be(ExpectedRoutes.HomeAction);
        redirectResult.ControllerName.Should().Be(ExpectedRoutes.HomeController);
    }

    [Fact]
    public void SignIn_WhenUserAlreadyAuthenticated_WithExternalUrl_RedirectsToHome()
    {
        var externalUrl = "https://malicious-site.com";
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(true);
        _mockUrlHelper.Setup(u => u.IsLocalUrl(externalUrl)).Returns(false);

        var result = _controller.SignIn(externalUrl);

        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be(ExpectedRoutes.HomeAction);
        redirectResult.ControllerName.Should().Be(ExpectedRoutes.HomeController);
    }

    [Fact]
    public void SignIn_WhenUserNotAuthenticated_ReturnsChallengeResult()
    {
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(false);

        var result = _controller.SignIn("/returnUrl");

        result.Should().BeOfType<ChallengeResult>();
        var challengeResult = result as ChallengeResult;
        challengeResult!.AuthenticationSchemes.Should().Contain(OpenIdConnectDefaults.AuthenticationScheme);
    }

    [Fact]
    public void SignIn_WhenUserNotAuthenticated_WithReturnUrl_SetsCorrectRedirectUri()
    {
        var returnUrl = "/custom/path";
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(false);

        var result = _controller.SignIn(returnUrl);

        result.Should().BeOfType<ChallengeResult>();
        var challengeResult = result as ChallengeResult;
        challengeResult!.Properties!.RedirectUri.Should().Be(returnUrl);
    }

    [Fact]
    public void SignIn_WhenUserNotAuthenticated_UsesOpenIdConnectScheme()
    {
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(false);

        var result = _controller.SignIn(null);

        result.Should().BeOfType<ChallengeResult>();
        var challengeResult = result as ChallengeResult;
        challengeResult!.AuthenticationSchemes.Should().HaveCount(1);
        challengeResult.AuthenticationSchemes.Should().Contain(OpenIdConnectDefaults.AuthenticationScheme);
    }

    #endregion

    #region SelectOrganisation GET Tests

    [Fact]
    public async Task SelectOrganisation_Get_WithNullUser_RedirectsToProblem()
    {
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User?)null);

        var result = await _controller.SelectOrganisation(null);

        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be(ExpectedRoutes.ErrorAction);
        redirectResult.ControllerName.Should().Be(ExpectedRoutes.ErrorController);
        redirectResult.RouteValues.Should().ContainKey("statusCode");
        redirectResult.RouteValues!["statusCode"].Should().Be(500);
    }

    [Fact]
    public async Task SelectOrganisation_Get_WithUserWithNoOrganisations_RedirectsToProblem()
    {
        var user = new User
        {
            Name = "test-user",
            Organisations = new List<Organisation>()
        };
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var result = await _controller.SelectOrganisation(null);

        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be(ExpectedRoutes.ErrorAction);
        redirectResult.ControllerName.Should().Be(ExpectedRoutes.ErrorController);
        redirectResult.RouteValues!["statusCode"].Should().Be(500);
    }

    [Fact]
    public async Task SelectOrganisation_Get_WithNullUser_LogsWarning()
    {
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User?)null);

        await _controller.SelectOrganisation(null);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("User not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SelectOrganisation_Get_WithValidUser_ReturnsView()
    {
        var user = new User
        {
            Name = "test-user",
            Organisations = new List<Organisation>
            {
                new() { Id = "org1", Name = "Organisation 1" }
            }
        };
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var result = await _controller.SelectOrganisation("/return");

        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        viewResult!.Model.Should().Be(user);
    }

    [Fact]
    public async Task SelectOrganisation_Get_WithValidUser_SetsReturnUrlInViewBag()
    {
        var user = new User
        {
            Name = "test-user",
            Organisations = new List<Organisation>
            {
                new() { Id = "org1", Name = "Organisation 1" }
            }
        };
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var result = await _controller.SelectOrganisation("/custom-return");

        result.Should().BeOfType<ViewResult>();
        ((string)_controller.ViewBag.ReturnUrl).Should().Be("/custom-return");
    }

    [Fact]
    public async Task SelectOrganisation_Get_WithMultipleOrganisations_ReturnsViewWithAllOrganisations()
    {
        var user = new User
        {
            Name = "test-user",
            Organisations = new List<Organisation>
            {
                new() { Id = "org1", Name = "Organisation 1" },
                new() { Id = "org2", Name = "Organisation 2" },
                new() { Id = "org3", Name = "Organisation 3" }
            }
        };
        _mockUserService.Setup(s => s.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var result = await _controller.SelectOrganisation(null);

        result.Should().BeOfType<ViewResult>();
        var viewResult = result as ViewResult;
        var model = viewResult!.Model as User;
        model!.Organisations.Should().HaveCount(3);
    }

    #endregion

    #region SelectOrganisationPost Tests

    [Fact]
    public async Task SelectOrganisationPost_WithEmptyOrganisationId_ReturnsBadRequest()
    {
        var result = await _controller.SelectOrganisationPost(string.Empty, "/return");

        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Organisation ID is required");
    }

    [Fact]
    public async Task SelectOrganisationPost_WithNullOrganisationId_ReturnsBadRequest()
    {
        var result = await _controller.SelectOrganisationPost(null!, "/return");

        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Organisation ID is required");
    }

    [Fact]
    public async Task SelectOrganisationPost_WhenSetOrganisationFails_RedirectsToProblem()
    {
        _mockUserService.Setup(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        var result = await _controller.SelectOrganisationPost("org-123", "/return");

        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be(ExpectedRoutes.ErrorAction);
        redirectResult.ControllerName.Should().Be(ExpectedRoutes.ErrorController);
        redirectResult.RouteValues!["statusCode"].Should().Be(500);
    }

    [Fact]
    public async Task SelectOrganisationPost_WhenSetOrganisationFails_LogsWarning()
    {
        _mockUserService.Setup(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        await _controller.SelectOrganisationPost("org-123", "/return");

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
    public async Task SelectOrganisationPost_WhenSuccessful_RedirectsToReturnUrl()
    {
        var returnUrl = "/dashboard";
        _mockUserService.Setup(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");
        _mockUrlHelper.Setup(u => u.IsLocalUrl(returnUrl)).Returns(true);

        var result = await _controller.SelectOrganisationPost("org-123", returnUrl);

        result.Should().BeOfType<RedirectResult>();
        var redirectResult = result as RedirectResult;
        redirectResult!.Url.Should().Be(returnUrl);
    }

    [Fact]
    public async Task SelectOrganisationPost_WhenSuccessful_WithNullReturnUrl_RedirectsToHome()
    {
        _mockUserService.Setup(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        var result = await _controller.SelectOrganisationPost("org-123", null);

        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be(ExpectedRoutes.HomeAction);
        redirectResult.ControllerName.Should().Be(ExpectedRoutes.HomeController);
    }

    [Fact]
    public async Task SelectOrganisationPost_WhenSuccessful_WithExternalReturnUrl_RedirectsToHome()
    {
        var externalUrl = "https://evil.com";
        _mockUserService.Setup(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");
        _mockUrlHelper.Setup(u => u.IsLocalUrl(externalUrl)).Returns(false);

        var result = await _controller.SelectOrganisationPost("org-123", externalUrl);

        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be(ExpectedRoutes.HomeAction);
        redirectResult.ControllerName.Should().Be(ExpectedRoutes.HomeController);
    }

    [Fact]
    public async Task SelectOrganisationPost_WhenSuccessful_LogsInformation()
    {
        _mockUserService.Setup(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        await _controller.SelectOrganisationPost("org-123", "/return");

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
    public async Task SelectOrganisationPost_CallsSetCurrentOrganisationWithCorrectParameters()
    {
        var organisationId = "org-test-123";
        _mockUserService.Setup(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        await _controller.SelectOrganisationPost(organisationId, "/return");

        _mockUserService.Verify(
            s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), organisationId),
            Times.Once);
    }

    #endregion

    #region SignOut Tests

    [Fact]
    public async Task SignOut_ClearsSession()
    {
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        await _controller.SignOut();

        _mockSession.Verify(s => s.Clear(), Times.Once);
    }

    [Fact]
    public async Task SignOut_ReturnsSignOutResult()
    {
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        var result = await _controller.SignOut();

        result.Should().BeOfType<SignOutResult>();
    }

    [Fact]
    public async Task SignOut_SignsOutOfOpenIdConnectScheme()
    {
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        var result = await _controller.SignOut();

        result.Should().BeOfType<SignOutResult>();
        var signOutResult = result as SignOutResult;
        signOutResult!.AuthenticationSchemes.Should().Contain(OpenIdConnectDefaults.AuthenticationScheme);
    }

    [Fact]
    public async Task SignOut_HasRedirectUriToSignedOut()
    {
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");
        _mockUrlHelper.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("/Auth/signed-out");

        var result = await _controller.SignOut();

        result.Should().BeOfType<SignOutResult>();
        var signOutResult = result as SignOutResult;
        signOutResult!.Properties.Should().NotBeNull();
        signOutResult.Properties!.RedirectUri.Should().Be("/Auth/signed-out");
    }

    [Fact]
    public async Task SignOut_LogsSignOutInformation()
    {
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        await _controller.SignOut();

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
    public async Task SignOut_LogsUserId()
    {
        var userId = "user-to-logout";
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);

        await _controller.SignOut();

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
    public async Task SignOut_ClearsSessionBeforeFederatedSignOut()
    {
        var sessionCleared = false;
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");
        _mockSession.Setup(s => s.Clear()).Callback(() => sessionCleared = true);

        var result = await _controller.SignOut();

        sessionCleared.Should().BeTrue("Session should be cleared before returning SignOutResult");
        result.Should().BeOfType<SignOutResult>();
    }

    #endregion

    #region AccessDenied Tests

    [Fact]
    public void AccessDenied_ReturnsView()
    {
        var result = _controller.AccessDenied();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void AccessDenied_ReturnsDefaultView()
    {
        var result = _controller.AccessDenied() as ViewResult;

        result!.ViewName.Should().BeNull();
    }

    #endregion

    #region SignedOut Tests

    [Fact]
    public void SignedOut_ReturnsView()
    {
        var result = _controller.SignedOut();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void SignedOut_ReturnsDefaultView()
    {
        var result = _controller.SignedOut() as ViewResult;

        result!.ViewName.Should().BeNull();
    }

    #endregion

    #region Security Tests

    [Theory]
    [InlineData("https://external.com")]
    [InlineData("//evil.com")]
    [InlineData("http://malicious.org/path")]
    public void SignIn_AuthenticatedUser_WithExternalUrl_RedirectsToHome(string externalUrl)
    {
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(true);
        _mockUrlHelper.Setup(u => u.IsLocalUrl(externalUrl)).Returns(false);

        var result = _controller.SignIn(externalUrl);

        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be(ExpectedRoutes.HomeAction);
        redirectResult.ControllerName.Should().Be(ExpectedRoutes.HomeController);
    }

    [Theory]
    [InlineData("/local/path")]
    [InlineData("/dashboard")]
    [InlineData("/school/search")]
    public void SignIn_AuthenticatedUser_WithLocalUrl_RedirectsToLocalUrl(string localUrl)
    {
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(true);
        _mockUrlHelper.Setup(u => u.IsLocalUrl(localUrl)).Returns(true);

        var result = _controller.SignIn(localUrl);

        result.Should().BeOfType<RedirectResult>();
        var redirectResult = result as RedirectResult;
        redirectResult!.Url.Should().Be(localUrl);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task SelectOrganisationPost_WithVeryLongOrganisationId_ProcessesCorrectly()
    {
        var longOrgId = new string('a', 1000);
        _mockUserService.Setup(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _mockUserService.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

        var result = await _controller.SelectOrganisationPost(longOrgId, "/return");

        result.Should().BeOfType<RedirectResult>();
        _mockUserService.Verify(s => s.SetCurrentOrganisationAsync(It.IsAny<ClaimsPrincipal>(), longOrgId), Times.Once);
    }

    [Fact]
    public void SignIn_WithSpecialCharactersInReturnUrl_ProcessesCorrectly()
    {
        var specialUrl = "/path?query=value&other=123";
        _mockUserService.Setup(s => s.IsAuthenticated(It.IsAny<ClaimsPrincipal>())).Returns(false);

        var result = _controller.SignIn(specialUrl);

        result.Should().BeOfType<ChallengeResult>();
        var challengeResult = result as ChallengeResult;
        challengeResult!.Properties!.RedirectUri.Should().Be(specialUrl);
    }

    #endregion
}