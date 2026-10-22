using FluentAssertions;
using SAPSec.Core.Authentication;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;

namespace SAPSec.Test.Integration.Tests;

[Collection("JsonRepositoryIntegrationTestsCollection")]
public class SchoolUserAccessIntegrationTests(JsonRepositoryIntegrationTestFixture fixture) : IAsyncLifetime
{
    private const string OwnUrn = "105574";
    private const string OtherUrn = "108088";
    private static readonly string SchoolOverviewPath = Routes.SecondarySchool(OwnUrn).Overview;

    public Task InitializeAsync()
    {
        fixture.TestUser.CategoryId = OrganisationCategoryIds.Establishment;
        fixture.TestUser.Urn = OwnUrn;
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        fixture.TestUser.Reset();
        return Task.CompletedTask;
    }

    [Theory]
    [InlineData("/find-a-school")]
    [InlineData("/find-a-school?query=Test")]
    [InlineData("/find-a-school/suggest?queryPart=Tes")]
    public async Task FindASchool_AsSchoolUser_ReturnsAccessDenied(string path)
    {
        var response = await fixture.NonRedirectingClient.GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task FindASchool_AsUserAllowedByPolicy_IsAvailable()
    {
        fixture.TestUser.Reset();

        var response = await fixture.NonRedirectingClient.GetAsync(Routes.FindASchool());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SchoolOverview_AsSchoolUser_DoesNotShowFindASchoolInNavigation()
    {
        var response = await fixture.Client.GetAsync(SchoolOverviewPath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotContain("Find a school");
        content.Should().Contain("Sign out");
    }

    [Fact]
    public async Task SchoolOverview_AsNonSchoolUser_ShowsFindASchoolInNavigation()
    {
        fixture.TestUser.Reset();

        var response = await fixture.Client.GetAsync(SchoolOverviewPath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Find a school");
    }

    [Fact]
    public async Task SchoolOverview_AsSchoolUser_ServiceNameLinksToUserHome()
    {
        var response = await fixture.Client.GetAsync(SchoolOverviewPath);
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain($"href=\"{Routes.UserHome}\"");
    }

    [Fact]
    public async Task UserHome_AsSchoolUser_RedirectsToTheirSchoolOverview()
    {
        var response = await fixture.NonRedirectingClient.GetAsync(Routes.UserHome);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.OriginalString.Should().Be(Routes.PrimarySchool(OwnUrn).Overview);
    }

    [Theory]
    [InlineData("Primary", "/school/primary/105574")]
    [InlineData("Secondary", "/school/secondary/105574")]
    [InlineData("All-through", "/school/all-through/105574")]
    public async Task UserHome_AsSchoolUser_RedirectsByPhase(string phase, string expectedPath)
    {
        fixture.TestUser.Phase = phase;

        var response = await fixture.NonRedirectingClient.GetAsync(Routes.UserHome);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.OriginalString.Should().Be(expectedPath);
    }

    [Theory]
    [InlineData("Nursery")]
    [InlineData("16 plus")]
    [InlineData("Not applicable")]
    public async Task UserHome_AsSchoolUserWithUnsupportedPhase_ReturnsNotFound(string phase)
    {
        fixture.TestUser.Phase = phase;

        var response = await fixture.NonRedirectingClient.GetAsync(Routes.UserHome);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UserHome_AsNonSchoolUser_ReturnsAccessDenied()
    {
        fixture.TestUser.Reset();

        var response = await fixture.NonRedirectingClient.GetAsync(Routes.UserHome);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #region School URN restriction

    [Fact]
    public async Task OwnSchool_PagesAreAvailable()
    {
        var overview = await fixture.NonRedirectingClient.GetAsync(Routes.SecondarySchool(OwnUrn).Overview);
        var similarSchools = await fixture.NonRedirectingClient.GetAsync(Routes.SecondarySchool(OwnUrn).ViewSimilarSchools);

        overview.StatusCode.Should().Be(HttpStatusCode.OK);
        similarSchools.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OwnSchool_ComparisonWithSimilarSchool_IsAvailable()
    {
        fixture.TestUser.Urn = OtherUrn;

        var response = await fixture.NonRedirectingClient.GetAsync(
            Routes.SecondarySchool(OtherUrn).Comparison("137621").KS4HeadlineMeasures);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AnotherSchool_ReturnsAccessDenied()
    {
        var response = await fixture.NonRedirectingClient.GetAsync(Routes.SecondarySchool(OtherUrn).Overview);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AnotherSchoolsSimilarSchoolsList_ReturnsAccessDenied()
    {
        // e.g. reached School B through a comparison, then tries to open School B's own similar-schools list.
        var response = await fixture.NonRedirectingClient.GetAsync(Routes.SecondarySchool(OtherUrn).ViewSimilarSchools);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AnotherSchoolsComparison_ReturnsAccessDenied()
    {
        var response = await fixture.NonRedirectingClient.GetAsync(
            Routes.SecondarySchool(OtherUrn).Comparison("137621").KS4HeadlineMeasures);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("/school/primary/999999")]
    [InlineData("/school/all-through/999999")]
    [InlineData("/school/primary/999999/view-similar-schools")]
    [InlineData("/school/all-through/999999/view-similar-schools")]
    public async Task AnotherSchool_AnyPhase_ReturnsAccessDenied(string path)
    {
        var response = await fixture.NonRedirectingClient.GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AnotherSchool_AsNonSchoolUser_IsNotRestrictedByThisRule()
    {
        fixture.TestUser.Reset();

        var response = await fixture.NonRedirectingClient.GetAsync(Routes.SecondarySchool(OtherUrn).Overview);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
