using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using SAPSec.Core.Constants;
using SAPSec.Web.Controllers;
using SAPSec.Web.Filters;
using AllThroughComparisonController = SAPSec.Web.Areas.AllThrough.Controllers.ComparisonController;
using AllThroughSchoolController = SAPSec.Web.Areas.AllThrough.Controllers.SchoolController;
using AllThroughSimilarSchoolsController = SAPSec.Web.Areas.AllThrough.Controllers.SimilarSchoolsController;
using PrimaryComparisonController = SAPSec.Web.Areas.Primary.Controllers.ComparisonController;
using PrimarySchoolController = SAPSec.Web.Areas.Primary.Controllers.SchoolController;
using PrimarySimilarSchoolsController = SAPSec.Web.Areas.Primary.Controllers.SimilarSchoolsController;
using SecondaryComparisonController = SAPSec.Web.Areas.Secondary.Controllers.ComparisonController;
using SecondarySchoolController = SAPSec.Web.Areas.Secondary.Controllers.SchoolController;
using SecondarySimilarSchoolsController = SAPSec.Web.Areas.Secondary.Controllers.SimilarSchoolsController;

namespace SAPSec.Web.Tests.Controllers;

public class SchoolRouteAuthorizationTests
{
    [Theory]
    [InlineData(typeof(SecondarySchoolController))]
    [InlineData(typeof(AllThroughSchoolController))]
    [InlineData(typeof(PrimarySimilarSchoolsController))]
    [InlineData(typeof(SecondarySimilarSchoolsController))]
    [InlineData(typeof(AllThroughSimilarSchoolsController))]
    [InlineData(typeof(PrimaryComparisonController))]
    [InlineData(typeof(SecondaryComparisonController))]
    [InlineData(typeof(AllThroughComparisonController))]
    public void SchoolRouteController_RequiresAuthorization(Type controllerType)
    {
        controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(typeof(SecondarySchoolController), ExpectedSchoolPhase.Secondary)]
    [InlineData(typeof(SecondarySimilarSchoolsController), ExpectedSchoolPhase.Secondary)]
    [InlineData(typeof(SecondaryComparisonController), ExpectedSchoolPhase.Secondary)]
    [InlineData(typeof(PrimarySchoolController), ExpectedSchoolPhase.Primary)]
    [InlineData(typeof(PrimarySimilarSchoolsController), ExpectedSchoolPhase.Primary)]
    [InlineData(typeof(PrimaryComparisonController), ExpectedSchoolPhase.Primary)]
    [InlineData(typeof(AllThroughSchoolController), ExpectedSchoolPhase.AllThrough)]
    [InlineData(typeof(AllThroughSimilarSchoolsController), ExpectedSchoolPhase.AllThrough)]
    [InlineData(typeof(AllThroughComparisonController), ExpectedSchoolPhase.AllThrough)]
    public void SchoolRouteController_UsesExpectedSchoolPhaseFilter(
        Type controllerType,
        ExpectedSchoolPhase expectedPhase)
    {
        var filter = controllerType
            .GetCustomAttributes(typeof(RequireSchoolPhaseAttribute), inherit: true)
            .OfType<RequireSchoolPhaseAttribute>()
            .SingleOrDefault(f =>
                f.Arguments is [ExpectedSchoolPhase phase, string[] routeParameterNames]
                && phase == expectedPhase
                && routeParameterNames.Contains("urn"));

        filter.Should().NotBeNull();
        filter!.Arguments.Should().NotBeNull();
        filter.Arguments![0].Should().Be(expectedPhase);
    }

    [Theory]
    [InlineData(typeof(AllThroughSchoolController))]
    [InlineData(typeof(AllThroughSimilarSchoolsController))]
    [InlineData(typeof(AllThroughComparisonController))]
    public void AllThroughController_RequiresAllThroughFeatureFlag(Type controllerType)
    {
        var filter = controllerType
            .GetCustomAttributes(typeof(RequireFeatureFlagAttribute), inherit: true)
            .OfType<RequireFeatureFlagAttribute>()
            .SingleOrDefault();

        filter.Should().NotBeNull();
        filter!.Arguments.Should().NotBeNull();
        filter.Arguments![0].Should().Be(FeatureFlags.EnableAllThroughSchools);
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
