using AngleSharp.Dom;
using FluentAssertions;
using SAPSec.Core.Authentication;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests;

/// <summary>
/// Find-a-school search is only for DfE users. Everyone else gets a 403 and no "Find a school" link.
/// </summary>
public class SchoolSearchAccessIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    private const string Urn = "100001";
    private const string TrustUid = "5001";

    private const string FindASchoolNavLink = "#navigation a[href='/find-a-school']";

    public override Task InitializeAsync()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment(Urn, "Test School", x => x.Open().Secondary().WithTrust(TrustUid)));

        return base.InitializeAsync();
    }

    public override Task DisposeAsync()
    {
        Fixture.TestUserOrganisation.Clear();
        Fixture.TestUserRoles.Reset();

        return base.DisposeAsync();
    }

    // The search index is shared between test classes, so use a query that matches no school;
    // a single match would redirect to that school instead of returning the results page.
    private const string NoMatchQuery = "zzqxj";

    public static TheoryData<string> SearchRoutes =>
    [
        Routes.FindASchool(),
        Routes.FindASchool(NoMatchQuery),
        Routes.FindASchoolSuggest(NoMatchQuery),
    ];

    [Theory]
    [MemberData(nameof(SearchRoutes))]
    public async Task Search_WhenEstablishmentUserWithoutDfeRole_IsForbidden(string path)
    {
        ActAsNonDfeUser(Organisation(OrganisationCategoryIds.Establishment));

        await AssertStatusCode(path, HttpStatusCode.Forbidden);
    }

    [Theory]
    [MemberData(nameof(SearchRoutes))]
    public async Task Search_WhenTrustUserWithoutDfeRole_IsForbidden(string path)
    {
        ActAsNonDfeUser(Organisation(OrganisationCategoryIds.MultiAcademyTrust, uid: TrustUid));

        await AssertStatusCode(path, HttpStatusCode.Forbidden);
    }

    [Theory]
    [MemberData(nameof(SearchRoutes))]
    public async Task Search_WhenDfeUser_IsAllowed(string path)
    {
        await AssertStatusCode(path, HttpStatusCode.OK);
    }

    [Fact]
    public async Task Navigation_WhenEstablishmentUserWithoutDfeRole_HidesFindASchoolLink()
    {
        ActAsNonDfeUser(Organisation(OrganisationCategoryIds.Establishment));

        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool(Urn).Overview);

        page.QuerySelector(FindASchoolNavLink).Should().BeNull();
        AssertHomeLinksGoToUserHome(page);
    }

    [Fact]
    public async Task Navigation_WhenTrustUserWithoutDfeRole_HidesFindASchoolLink()
    {
        ActAsNonDfeUser(Organisation(OrganisationCategoryIds.MultiAcademyTrust, uid: TrustUid));

        var page = await Fixture.RequestPageAsync(Routes.YourSchools);

        page.QuerySelector(FindASchoolNavLink).Should().BeNull();
        page.QuerySelector("a[href^='/find-a-school']").Should().BeNull();
    }

    [Fact]
    public async Task Navigation_WhenDfeUser_ShowsFindASchoolLink()
    {
        var page = await Fixture.RequestPageAsync(Routes.SecondarySchool(Urn).Overview);

        page.QuerySelector(FindASchoolNavLink).Should().NotBeNull();
    }

    [Fact]
    public async Task UserRedirect_WhenOtherOrganisationWithoutDfeRole_IsForbidden()
    {
        ActAsNonDfeUser(Organisation("011"));

        await AssertStatusCode(Routes.UserHome, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UserRedirect_WhenOtherOrganisationWithDfeRole_RedirectsToFindASchool()
    {
        Fixture.TestUserOrganisation.Set(Organisation("011"));

        var response = await Fixture.NonRedirectingClient.GetAsync(Routes.UserHome);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.OriginalString.Should().Be(Routes.FindASchool());
    }

    private void ActAsNonDfeUser(Organisation organisation)
    {
        Fixture.TestUserOrganisation.Set(organisation);
        Fixture.TestUserRoles.Set();
    }

    private async Task AssertStatusCode(string path, HttpStatusCode expectedStatusCode)
    {
        var response = await Fixture.NonRedirectingClient.GetAsync(path);

        response.StatusCode.Should().Be(expectedStatusCode, because: $"of the response to {path}");
    }

    private static void AssertHomeLinksGoToUserHome(IDocument page)
    {
        page.QuerySelector(".govuk-service-navigation__service-name a")!.GetAttribute("href")
            .Should().Be(Routes.UserHome);
        page.QuerySelectorAll(".govuk-breadcrumbs__link")
            .Where(a => a.TextContent.Trim() == "Home")
            .Select(a => a.GetAttribute("href"))
            .Should().AllBe(Routes.UserHome);
    }

    private static Organisation Organisation(string categoryId, string? uid = null) =>
        new()
        {
            Id = "test-org",
            Name = "Test Organisation",
            Category = new Category { Id = categoryId },
            Uid = uid
        };
}
