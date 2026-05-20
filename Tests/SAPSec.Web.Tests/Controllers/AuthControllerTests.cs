using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Interfaces.Services;
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
        public const string DefaultReturnUrl = "/find-a-school";
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
        _mockUrlHelper.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns("/auth/signed-out");

        var result = await _controller.SignOut();

        result.Should().BeOfType<SignOutResult>();
        var signOutResult = result as SignOutResult;
        signOutResult!.Properties.Should().NotBeNull();
        signOutResult.Properties!.RedirectUri.Should().Be("/auth/signed-out");
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
    [InlineData("/find-a-school/search")]
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