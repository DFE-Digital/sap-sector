using FluentAssertions;
using SAPSec.Core.Authentication;
using SAPSec.Test.Common.Authentication;
using SAPSec.Core.FeatureFlags;
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
        fixture.TestUser.Email = TestUserOrganisation.NonDfeEmail;
        fixture.TestUser.Urn = OwnUrn;
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        fixture.FeatureFlagService.ClearOverrides(Flags.EnableFindASchool);
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
    public async Task FindASchool_AsDfeUser_IsAvailable()
    {
        fixture.TestUser.Reset();

        var response = await fixture.NonRedirectingClient.GetAsync(Routes.FindASchool());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task FindASchool_AsSchoolUserWithDfeEmail_IsAvailable()
    {
        // DfE staff are recognised by email, so signing in for a school does not take Find a school away.
        fixture.TestUser.Email = TestUserOrganisation.DfeEmail;

        var response = await fixture.NonRedirectingClient.GetAsync(Routes.FindASchool());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(OrganisationCategoryIds.LocalAuthority)]
    [InlineData(OrganisationCategoryIds.MultiAcademyTrust)]
    [InlineData(OrganisationCategoryIds.SingleAcademyTrust)]
    public async Task FindASchool_AsNonDfeUserWhoIsNotASchool_ReturnsAccessDenied(string categoryId)
    {
        ActAsNonDfeUser(categoryId);

        var response = await fixture.NonRedirectingClient.GetAsync(Routes.FindASchool());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
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
    public async Task SchoolOverview_AsDfeUser_ShowsFindASchoolInNavigation()
    {
        fixture.TestUser.Reset();

        var response = await fixture.Client.GetAsync(SchoolOverviewPath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Find a school");
    }

    [Fact]
    public async Task SchoolOverview_AsNonDfeUserWhoIsNotASchool_DoesNotShowFindASchoolInNavigation()
    {
        ActAsNonDfeUser(OrganisationCategoryIds.LocalAuthority);

        var response = await fixture.Client.GetAsync(SchoolOverviewPath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotContain("Find a school");
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
    [InlineData("")]
    [InlineData("Nursery")]
    [InlineData("Middle deemed secondary")]
    [InlineData("Not applicable")]
    public async Task UserHome_AsSchoolUserWithUnrecognisedPhase_StillReachesTheSchoolsRealPage(string phase)
    {
        fixture.TestUser.Phase = phase;

        var response = await fixture.Client.GetAsync(Routes.UserHome);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.RequestMessage!.RequestUri!.AbsolutePath.Should().Be(Routes.SecondarySchool(OwnUrn).Overview);
    }

    [Fact]
    public async Task UserHome_AsNonDfeUserWhoIsNotASchool_ReturnsAccessDenied()
    {
        ActAsNonDfeUser(OrganisationCategoryIds.LocalAuthority);

        var response = await fixture.NonRedirectingClient.GetAsync(Routes.UserHome);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UserHome_AsDfeUser_RedirectsToFindASchool()
    {
        fixture.TestUser.Reset();

        var response = await fixture.NonRedirectingClient.GetAsync(Routes.UserHome);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.OriginalString.Should().Be(Routes.FindASchool());
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

    #region Breadcrumbs

    public static TheoryData<string> BreadcrumbPages() =>
    [
        Routes.SecondarySchool("105574").Overview,
        Routes.SecondarySchool("105574").KS4HeadlineMeasures,
        Routes.SecondarySchool("105574").ViewSimilarSchools,
        Routes.SecondarySchool("105574").WhatIsASimilarSchool,
        Routes.SecondarySchool("105574").SchoolDetails,
        "/cookies",
        Routes.TermsAndConditions
    ];

    [Theory]
    [MemberData(nameof(BreadcrumbPages))]
    public async Task Pages_AsSchoolUser_DoNotShowBreadcrumbs(string path)
    {
        var response = await fixture.Client.GetAsync(path);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotContain("govuk-breadcrumbs");
    }

    [Theory]
    [MemberData(nameof(BreadcrumbPages))]
    public async Task Pages_AsDfeUser_StillShowBreadcrumbs(string path)
    {
        fixture.TestUser.Reset();

        var response = await fixture.Client.GetAsync(path);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("govuk-breadcrumbs");
    }

    [Theory]
    [MemberData(nameof(BreadcrumbPages))]
    public async Task Pages_AsNonDfeUserWhoIsNotASchool_DoNotShowBreadcrumbs(string path)
    {
        // The "Home" breadcrumb leads to Find a school, which these users cannot use.
        ActAsNonDfeUser(OrganisationCategoryIds.LocalAuthority);

        var response = await fixture.Client.GetAsync(path);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotContain("govuk-breadcrumbs");
    }

    [Fact]
    public async Task ComparisonPage_AsSchoolUser_HasBackButtonButNoBreadcrumbs()
    {
        fixture.TestUser.Urn = OtherUrn;

        var response = await fixture.Client.GetAsync(
            Routes.SecondarySchool(OtherUrn).Comparison("137621").KS4HeadlineMeasures);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotContain("govuk-breadcrumbs");
        content.Should().Contain("govuk-back-link");
        System.Text.RegularExpressions.Regex.IsMatch(content, @"govuk-back-link[^>]*>\s*View similar schools\s*<")
            .Should().BeTrue("the back link names the page it returns to");
        content.Should().Contain(Routes.SecondarySchool(OtherUrn).ViewSimilarSchools);
    }

    #endregion

    #region Feature flag off

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AccessDenied_ContentFollowsFindASchoolFlag(bool enableFindASchool)
    {
        fixture.FeatureFlagService.Override(Flags.EnableFindASchool, enableFindASchool);
        var response = await fixture.NonRedirectingClient.GetAsync("/error/403");
        var content = await response.Content.ReadAsStringAsync();

        if (enableFindASchool)
        {
            content.Should().Contain("assign your DfE Sign-in to an organisation");
            content.Should().Contain("help with DfE Sign-in");
            content.Should().NotContain("Access for establishment groups");
        }
        else
        {
            content.Should().Contain("Access for establishment groups");
            content.Should().Contain("Request access to an organisation");
            content.Should().NotContain("assign your DfE Sign-in to an organisation");
        }
    }

    [Fact]
    public async Task WhenRestrictionsAreSwitchedOff_SchoolUserCanStillUseFindASchool()
    {
        fixture.FeatureFlagService.Override(Flags.EnableFindASchool, true);

        var response = await fixture.NonRedirectingClient.GetAsync(Routes.FindASchool());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WhenRestrictionsAreSwitchedOff_SchoolUserCanOpenAnotherSchool_AndSeesFindASchoolInNavigation()
    {
        fixture.FeatureFlagService.Override(Flags.EnableFindASchool, true);

        var response = await fixture.Client.GetAsync(Routes.SecondarySchool(OtherUrn).Overview);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Find a school");
    }

    [Fact]
    public async Task ServiceName_WhenFindASchoolIsEnabled_LinksToFindASchool_ForEveryone()
    {
        fixture.FeatureFlagService.Override(Flags.EnableFindASchool, true);

        var schoolUser = await fixture.Client.GetStringAsync(SchoolOverviewPath);
        fixture.TestUser.Reset();
        var otherUser = await fixture.Client.GetStringAsync(SchoolOverviewPath);

        ServiceNameHref(schoolUser).Should().Be(Routes.FindASchool());
        ServiceNameHref(otherUser).Should().Be(Routes.FindASchool());
    }

    [Fact]
    public async Task WhenRestrictionsAreSwitchedOff_SchoolUserStillSeesBreadcrumbs()
    {
        fixture.FeatureFlagService.Override(Flags.EnableFindASchool, true);

        var response = await fixture.Client.GetAsync(SchoolOverviewPath);
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("govuk-breadcrumbs");
    }

    [Fact]
    public async Task WhenRestrictionsAreSwitchedOff_UserHomeRoutesAsBefore()
    {
        fixture.FeatureFlagService.Override(Flags.EnableFindASchool, true);

        var schoolUser = await fixture.NonRedirectingClient.GetAsync(Routes.UserHome);
        fixture.TestUser.Reset();
        var otherUser = await fixture.NonRedirectingClient.GetAsync(Routes.UserHome);

        schoolUser.Headers.Location!.OriginalString.Should().Be(Routes.PrimarySchool(OwnUrn).Overview);
        otherUser.StatusCode.Should().Be(HttpStatusCode.Redirect);
        otherUser.Headers.Location!.OriginalString.Should().Be(Routes.FindASchool());
    }


    #endregion

    private void ActAsNonDfeUser(string categoryId)
    {
        fixture.TestUser.Reset();
        fixture.TestUser.CategoryId = categoryId;
        fixture.TestUser.Email = TestUserOrganisation.NonDfeEmail;
    }

    private static string ServiceNameHref(string content) =>
        System.Text.RegularExpressions.Regex.Match(content, @"govuk-service-navigation__service-name"">\s*<a[^>]*href=""([^""]*)""").Groups[1].Value;
}
