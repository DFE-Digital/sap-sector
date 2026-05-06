using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using SAPSec.Web.Controllers;

namespace SAPSec.Web.Tests.Controllers;

public class SchoolRouteAuthorizationTests
{
    [Theory]
    [InlineData(typeof(SchoolController))]
    [InlineData(typeof(SimilarSchoolsController))]
    [InlineData(typeof(SimilarSchoolsComparisonController))]
    public void SchoolRouteController_RequiresAuthorization(Type controllerType)
    {
        controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(typeof(HomeController))]
    [InlineData(typeof(ErrorController))]
    [InlineData(typeof(HealthController))]
    [InlineData(typeof(StaticContentController))]
    public void PublicController_AllowsAnonymousAccess(Type controllerType)
    {
        controllerType.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true)
            .Should().NotBeEmpty();
    }

    [Fact]
    public void SignIn_AllowsAnonymousAccess()
    {
        typeof(AuthController)
            .GetMethod(nameof(AuthController.SignIn), [typeof(string)])
            .Should().NotBeNull()
            .And.Subject!
            .GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true)
            .Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(nameof(AuthController.SignedOut))]
    public void PublicAuthAction_AllowsAnonymousAccess(string actionName)
    {
        typeof(AuthController)
            .GetMethod(actionName, Type.EmptyTypes)
            .Should().NotBeNull()
            .And.Subject!
            .GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true)
            .Should().NotBeEmpty();
    }
}
