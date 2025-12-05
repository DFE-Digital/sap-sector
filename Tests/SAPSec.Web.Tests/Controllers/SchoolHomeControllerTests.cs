using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Web.Controllers;
using System.Security.Claims;

namespace SAPSec.Web.Tests.Controllers;

public class SchoolHomeControllerTests
{
    private readonly IUserService _userService;
    private readonly ILogger<SchoolHomeController> _logger;
    private readonly SchoolHomeController _sut; 

    public SchoolHomeControllerTests()
    {
        _userService = Substitute.For<IUserService>();
        _logger = Substitute.For<ILogger<SchoolHomeController>>();
        _sut = CreateController();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullUserService_ThrowsArgumentNullException()
    {
        var action = () => new SchoolHomeController(null!, _logger);

        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("userService");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        var action = () => new SchoolHomeController(_userService, null!);

        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var controller = new SchoolHomeController(_userService, _logger);

        controller.Should().NotBeNull();
    }

    #endregion

    #region Index - Establishment User (Happy Path)

    [Fact]
    public async Task Index_WhenUserIsEstablishment_ReturnsView()
    {
        SetupEstablishmentUser("Test Academy", "John Smith");

        var result = await _sut.Index();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Index_WhenUserIsEstablishment_SetsSchoolNameInViewBag()
    {
        SetupEstablishmentUser("Meadow Primary School", "John Smith");

        await _sut.Index();

        GetViewBagSchoolName().Should().Be("Meadow Primary School");
    }

    [Fact]
    public async Task Index_WhenUserIsEstablishment_SetsUserNameInViewBag()
    {
        SetupEstablishmentUser("Test School", "Jane Doe");

        await _sut.Index();

        GetViewBagUserName().Should().Be("Jane Doe");
    }

    [Fact]
    public async Task Index_WhenOrganisationNotInUserList_SetsDefaultSchoolName()
    {
        SetupUserWithDifferentCurrentOrganisation();

        await _sut.Index();

        GetViewBagSchoolName().Should().Be("School");
    }

    #endregion

    #region Index - Non-Establishment User (Redirect to SchoolSearch)

    [Fact]
    public async Task Index_WhenUserIsLocalAuthority_RedirectsToSchoolSearch()
    {
        SetupNonEstablishmentUser(TestData.Categories.LocalAuthority);

        var result = await _sut.Index();

        AssertRedirectsToSchoolSearch(result);
    }

    [Fact]
    public async Task Index_WhenUserIsMultiAcademyTrust_RedirectsToSchoolSearch()
    {
        SetupNonEstablishmentUser(TestData.Categories.MultiAcademyTrust);

        var result = await _sut.Index();

        AssertRedirectsToSchoolSearch(result);
    }

    [Theory]
    [InlineData("Local Authority")]
    [InlineData("Multi-Academy Trust")]
    [InlineData("Single-Academy Trust")]
    [InlineData("Federation")]
    [InlineData("Other")]
    public async Task Index_WhenUserIsNonEstablishment_RedirectsToSchoolSearch(string categoryName)
    {
        SetupNonEstablishmentUser(categoryName);

        var result = await _sut.Index();

        AssertRedirectsToSchoolSearch(result);
    }

    #endregion

    #region Index - Null/Invalid Organisation (Access Denied)

    [Fact]
    public async Task Index_WhenOrganisationIsNull_RedirectsToAccessDenied()
    {
        SetupUserWithNullOrganisation();

        var result = await _sut.Index();

        AssertRedirectsToAccessDenied(result);
    }

    [Fact]
    public async Task Index_WhenOrganisationCategoryIsNull_RedirectsToAccessDenied()
    {
        SetupUserWithNullCategory();

        var result = await _sut.Index();

        AssertRedirectsToAccessDenied(result);
    }

    [Fact]
    public async Task Index_WhenUserHasNoOrganisations_RedirectsToAccessDenied()
    {
        SetupUserWithNoOrganisations();

        var result = await _sut.Index();

        AssertRedirectsToAccessDenied(result);
    }

    #endregion

    #region Index - Service Call Verification

    [Fact]
    public async Task Index_Always_CallsGetUserFromClaimsAsync()
    {
        SetupEstablishmentUser();

        await _sut.Index();

        await _userService.Received(1).GetUserFromClaimsAsync(Arg.Any<ClaimsPrincipal>());
    }

    [Fact]
    public async Task Index_Always_CallsGetCurrentOrganisationAsync()
    {
        SetupEstablishmentUser();

        await _sut.Index();

        await _userService.Received(1).GetCurrentOrganisationAsync(Arg.Any<ClaimsPrincipal>());
    }

    [Fact]
    public async Task Index_WhenOrganisationIsNull_StillCallsBothServiceMethods()
    {
        SetupUserWithNullOrganisation();

        await _sut.Index();

        await _userService.Received(1).GetUserFromClaimsAsync(Arg.Any<ClaimsPrincipal>());
        await _userService.Received(1).GetCurrentOrganisationAsync(Arg.Any<ClaimsPrincipal>());
    }

    #endregion

    #region Index - Multiple Organisations

    [Fact]
    public async Task Index_WhenUserHasMultipleOrganisations_UsesCurrentOrganisation()
    {
        SetupUserWithMultipleOrganisations(currentSchoolName: "Second School");

        await _sut.Index();

        GetViewBagSchoolName().Should().Be("Second School");
    }

    [Fact]
    public async Task Index_WhenMixedOrganisationTypes_AndCurrentIsEstablishment_ReturnsView()
    {
        SetupUserWithMixedOrganisations(currentIsEstablishment: true);

        var result = await _sut.Index();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Index_WhenMixedOrganisationTypes_AndCurrentIsNonEstablishment_RedirectsToSchoolSearch()
    {
        SetupUserWithMixedOrganisations(currentIsEstablishment: false);

        var result = await _sut.Index();

        AssertRedirectsToSchoolSearch(result);
    }

    #endregion

    #region Index - Case Insensitivity

    [Fact]
    public async Task Index_WhenCategoryNameIsLowercase_StillTreatsAsEstablishment()
    {
        SetupUserWithCategoryName("establishment");

        var result = await _sut.Index();

        result.Should().BeOfType<ViewResult>();
    }

    [Theory]
    [InlineData("Establishment")]
    [InlineData("establishment")]
    [InlineData("ESTABLISHMENT")]
    [InlineData("EstablishMent")]
    public async Task Index_WhenCategoryNameHasVariousCasing_TreatsAsEstablishment(string categoryName)
    {
        SetupUserWithCategoryName(categoryName);

        var result = await _sut.Index();

        result.Should().BeOfType<ViewResult>();
    }

    #endregion

    #region ViewBag Tests

    [Fact]
    public async Task Index_WhenEstablishment_ViewBagContainsBothProperties()
    {
        SetupEstablishmentUser("Oak Tree Academy", "Head Teacher");

        await _sut.Index();

        GetViewBagSchoolName().Should().Be("Oak Tree Academy");
        GetViewBagUserName().Should().Be("Head Teacher");
    }

    #endregion

    #region Test Setup Helpers

    private SchoolHomeController CreateController()
    {
        var controller = new SchoolHomeController(_userService, _logger);
        var httpContext = new DefaultHttpContext { User = TestData.CreatePrincipal() };

        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, Substitute.For<ITempDataProvider>());

        return controller;
    }

    private void SetupEstablishmentUser(
        string schoolName = "Test School",
        string userName = "Test User")
    {
        var orgId = Guid.NewGuid().ToString();
        var organisation = TestData.CreateOrganisation(schoolName, TestData.Categories.Establishment, orgId);
        var user = TestData.CreateUser(userName, new List<Organisation> { organisation });

        SetupMocks(user, organisation);
    }

    private void SetupNonEstablishmentUser(string categoryName)
    {
        var orgId = Guid.NewGuid().ToString();
        var organisation = TestData.CreateOrganisation("Test Org", categoryName, orgId);
        var user = TestData.CreateUser("Test User", new List<Organisation> { organisation });

        SetupMocks(user, organisation);
    }

    private void SetupUserWithNullOrganisation()
    {
        var user = TestData.CreateUser("Test User");

        _userService.GetUserFromClaimsAsync(Arg.Any<ClaimsPrincipal>()).Returns(user);
        _userService.GetCurrentOrganisationAsync(Arg.Any<ClaimsPrincipal>()).Returns((Organisation?)null);
    }

    private void SetupUserWithNullCategory()
    {
        var organisation = TestData.CreateOrganisationWithNullCategory();
        var user = TestData.CreateUser("Test User", new List<Organisation> { organisation });

        SetupMocks(user, organisation);
    }

    private void SetupUserWithNoOrganisations()
    {
        var user = TestData.CreateUser("Test User", new List<Organisation>());

        _userService.GetUserFromClaimsAsync(Arg.Any<ClaimsPrincipal>()).Returns(user);
        _userService.GetCurrentOrganisationAsync(Arg.Any<ClaimsPrincipal>()).Returns((Organisation?)null);
    }

    private void SetupUserWithDifferentCurrentOrganisation()
    {
        var userOrganisation = TestData.CreateOrganisation("User's School", TestData.Categories.Establishment);
        var currentOrganisation = TestData.CreateOrganisation("Different School", TestData.Categories.Establishment);
        var user = TestData.CreateUser("Test User", new List<Organisation> { userOrganisation });

        _userService.GetUserFromClaimsAsync(Arg.Any<ClaimsPrincipal>()).Returns(user);
        _userService.GetCurrentOrganisationAsync(Arg.Any<ClaimsPrincipal>()).Returns(currentOrganisation);
    }

    private void SetupUserWithMultipleOrganisations(string currentSchoolName)
    {
        var school1 = TestData.CreateOrganisation("First School", TestData.Categories.Establishment);
        var school2 = TestData.CreateOrganisation(currentSchoolName, TestData.Categories.Establishment);
        var user = TestData.CreateUser("Multi-Org User", new List<Organisation> { school1, school2 });

        _userService.GetUserFromClaimsAsync(Arg.Any<ClaimsPrincipal>()).Returns(user);
        _userService.GetCurrentOrganisationAsync(Arg.Any<ClaimsPrincipal>()).Returns(school2);
    }

    private void SetupUserWithMixedOrganisations(bool currentIsEstablishment)
    {
        var schoolId = Guid.NewGuid().ToString();
        var trustId = Guid.NewGuid().ToString();
        var school = TestData.CreateOrganisation("My School", TestData.Categories.Establishment, schoolId);
        var trust = TestData.CreateOrganisation("My Trust", TestData.Categories.MultiAcademyTrust, trustId);
        var user = TestData.CreateUser("Mixed User", new List<Organisation> { school, trust });

        var currentOrg = currentIsEstablishment ? school : trust;

        _userService.GetUserFromClaimsAsync(Arg.Any<ClaimsPrincipal>()).Returns(user);
        _userService.GetCurrentOrganisationAsync(Arg.Any<ClaimsPrincipal>()).Returns(currentOrg);
    }

    private void SetupUserWithCategoryName(string categoryName)
    {
        var orgId = Guid.NewGuid().ToString();
        var organisation = TestData.CreateOrganisation("Test School", categoryName, orgId);
        var user = TestData.CreateUser("Test User", new List<Organisation> { organisation });

        SetupMocks(user, organisation);
    }

    private void SetupMocks(User user, Organisation organisation)
    {
        _userService.GetUserFromClaimsAsync(Arg.Any<ClaimsPrincipal>()).Returns(user);
        _userService.GetCurrentOrganisationAsync(Arg.Any<ClaimsPrincipal>()).Returns(organisation);
    }

    #endregion

    #region Assertion Helpers

    private string GetViewBagSchoolName() => (string)_sut.ViewBag.SchoolName;

    private string GetViewBagUserName() => (string)_sut.ViewBag.UserName;

    private static void AssertRedirectsToSchoolSearch(IActionResult result)
    {
        result.Should().BeOfType<RedirectResult>();
        var redirectResult = result as RedirectResult;
        redirectResult!.Url.Should().Be(TestData.Routes.SchoolSearch);
    }

    private static void AssertRedirectsToAccessDenied(IActionResult result)
    {
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = result as RedirectToActionResult;
        redirectResult!.ActionName.Should().Be(TestData.Routes.ErrorAction);
        redirectResult.ControllerName.Should().Be(TestData.Routes.ErrorController);
        redirectResult.RouteValues!["statusCode"].Should().Be(TestData.StatusCodes.AccessDenied);
    }

    #endregion

    #region Test Data

    private static class TestData
    {
        public static class Routes
        {
            public const string SchoolSearch = "/search-for-a-school";
            public const string ErrorAction = "StatusCodeError";
            public const string ErrorController = "Error";
        }

        public static class StatusCodes
        {
            public const int AccessDenied = 403;
        }

        public static class Categories
        {
            public const string Establishment = "Establishment";
            public const string LocalAuthority = "Local Authority";
            public const string MultiAcademyTrust = "Multi-Academy Trust";
        }

        public static ClaimsPrincipal CreatePrincipal(string userId = "test-user-id")
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, "Test User"),
                new(ClaimTypes.Email, "test@example.com")
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        public static User CreateUser(string name = "Test User", List<Organisation>? organisations = null)
        {
            return new User
            {
                Name = name,
                Email = "test@example.com",
                Organisations = organisations ?? new List<Organisation>()
            };
        }

        public static Organisation CreateOrganisation(
            string name = "Test School",
            string categoryName = Categories.Establishment,
            string? id = null)
        {
            return new Organisation
            {
                Id = id ?? Guid.NewGuid().ToString(),
                Name = name,
                Category = new Category { Id = "1", Name = categoryName }
            };
        }

        public static Organisation CreateOrganisationWithNullCategory(string name = "Test Org")
        {
            return new Organisation
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Category = null
            };
        }
    }

    #endregion
}