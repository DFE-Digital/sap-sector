using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using SAPSec.Web.Controllers;
using SAPSec.Web.Filters;
using PrimarySchoolController = SAPSec.Web.Areas.Primary.Controllers.SchoolController;
using SecondarySchoolController = SAPSec.Web.Controllers.SchoolController;

namespace SAPSec.Web.Tests.Controllers;

public class SchoolRouteAuthorizationTests
{
    [Theory]
    [InlineData(typeof(SecondarySchoolController))]
    [InlineData(typeof(SimilarSchoolsController))]
    [InlineData(typeof(SimilarSchoolsComparisonController))]
    public void SchoolRouteController_RequiresAuthorization(Type controllerType)
    {
        controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(typeof(SecondarySchoolController), ExpectedSchoolPhase.Secondary)]
    [InlineData(typeof(SimilarSchoolsController), ExpectedSchoolPhase.Secondary)]
    [InlineData(typeof(SimilarSchoolsComparisonController), ExpectedSchoolPhase.Secondary)]
    [InlineData(typeof(PrimarySchoolController), ExpectedSchoolPhase.Primary)]
    public void SchoolRouteController_UsesExpectedSchoolPhaseFilter(
        Type controllerType,
        ExpectedSchoolPhase expectedPhase)
    {
        var filter = controllerType
            .GetCustomAttributes(typeof(RequireSchoolPhaseAttribute), inherit: true)
            .OfType<RequireSchoolPhaseAttribute>()
            .SingleOrDefault();

        filter.Should().NotBeNull();
        filter!.Arguments.Should().NotBeNull();
        filter.Arguments![0].Should().Be(expectedPhase);
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
