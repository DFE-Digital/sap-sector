using FluentAssertions;
using SAPSec.Core.Authentication;
using SAPSec.Core.FeatureFlags;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using Xunit.Abstractions;

namespace SAPSec.Test.Integration.Tests;

public class YourSchoolsPageIntegrationTests(
    InMemoryRepositoryIntegrationTestFixture fixture,
    ITestOutputHelper outputHelper) : InMemoryRepositoryIntegrationTests(fixture, outputHelper)
{
    private const string UsersTrustUid = "5001";
    private const string UsersLaCode = "201";

    public override Task InitializeAsync()
    {
        Fixture.FeatureFlagService.Override(Flags.EnablePrimarySchools, true);
        Fixture.FeatureFlagService.Override(Flags.EnableAllThroughSchools, true);

        Fixture.EstablishmentRepository.SetupEstablishments(
            Build.Establishment("100001", "Trust Primary", x => x.Open().Primary().WithTrust(UsersTrustUid).InLA("999", "Elsewhere")),
            Build.Establishment("100002", "Trust Secondary", x => x.Open().Secondary().WithTrust(UsersTrustUid).InLA(UsersLaCode, "Users LA")),
            Build.Establishment("100003", "Other Trust Secondary", x => x.Open().Secondary().WithTrust("5002").InLA(UsersLaCode, "Users LA")),
            Build.Establishment("100004", "LA Maintained Primary", x => x.Open().Primary().InLA(UsersLaCode, "Users LA")),
            Build.Establishment("100005", "Other LA Primary", x => x.Open().Primary().InLA("202", "Other LA")));

        return base.InitializeAsync();
    }

    public override Task DisposeAsync()
    {
        Fixture.TestUserOrganisation.Clear();
        Fixture.FeatureFlagService.ClearOverrides(Flags.EnablePrimarySchools);
        Fixture.FeatureFlagService.ClearOverrides(Flags.EnableAllThroughSchools);

        return base.DisposeAsync();
    }

    [Fact]
    public async Task YourSchools_WhenTrustUser_ListsOnlySchoolsInTheirTrust()
    {
        Fixture.TestUserOrganisation.Set(Organisation("Test Trust", OrganisationCategoryIds.MultiAcademyTrust, uid: UsersTrustUid));

        var page = await Fixture.RequestPageAsync(Routes.YourSchools);

        page.QuerySelector("h1")!.TextContent.Trim().Should().Be("Your schools");
        page.QuerySelector("#organisation-name")!.TextContent.Trim().Should().Be("Test Trust");
        SchoolNames(page).Should().Equal("Trust Primary", "Trust Secondary");
        page.QuerySelector("a[href='/school/primary/100001']").Should().NotBeNull();
        page.QuerySelector("a[href='/school/secondary/100002']").Should().NotBeNull();
    }

    [Fact]
    public async Task YourSchools_WhenLocalAuthorityUser_ListsAllSchoolsInTheirLocalAuthority()
    {
        Fixture.TestUserOrganisation.Set(Organisation("Users LA", OrganisationCategoryIds.LocalAuthority, establishmentNumber: UsersLaCode));

        var page = await Fixture.RequestPageAsync(Routes.YourSchools);

        SchoolNames(page).Should().Equal("LA Maintained Primary", "Other Trust Secondary", "Trust Secondary");
    }

    [Fact]
    public async Task YourSchools_WhenOrganisationHasNoSchools_ShowsNoSchoolsMessage()
    {
        Fixture.TestUserOrganisation.Set(Organisation("Empty Trust", OrganisationCategoryIds.MultiAcademyTrust, uid: "9999"));

        var page = await Fixture.RequestPageAsync(Routes.YourSchools);

        SchoolNames(page).Should().BeEmpty();
        page.QuerySelector("#no-schools-message")!.TextContent.Should().Contain("Empty Trust");
    }

    [Fact]
    public async Task YourSchools_WhenMoreThanOnePage_PaginatesWithinYourSchools()
    {
        Fixture.EstablishmentRepository.SetupEstablishments(
            Enumerable.Range(1, 12)
                .Select(i => Build.Establishment($"1000{i:00}", $"Trust School {i:00}", x => x.Open().Secondary().WithTrust(UsersTrustUid)))
                .ToArray());
        Fixture.TestUserOrganisation.Set(Organisation("Big Trust", OrganisationCategoryIds.MultiAcademyTrust, uid: UsersTrustUid));

        var firstPage = await Fixture.RequestPageAsync(Routes.YourSchools);
        var secondPage = await Fixture.RequestPageAsync($"{Routes.YourSchools}?page=2");

        SchoolNames(firstPage).Should().HaveCount(10);
        firstPage.QuerySelector("a[rel='next']")!.GetAttribute("href").Should().Be($"{Routes.YourSchools}?page=2");
        SchoolNames(secondPage).Should().Equal("Trust School 11", "Trust School 12");
    }

    [Theory]
    [InlineData(OrganisationCategoryIds.Establishment)]
    [InlineData("011")]
    public async Task YourSchools_WhenNotTrustOrLocalAuthorityUser_IsForbidden(string categoryId)
    {
        Fixture.TestUserOrganisation.Set(Organisation("Not A Trust", categoryId));

        var response = await Fixture.NonRedirectingClient.GetAsync(Routes.YourSchools);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData(OrganisationCategoryIds.MultiAcademyTrust, UsersTrustUid, null)]
    [InlineData(OrganisationCategoryIds.LocalAuthority, null, UsersLaCode)]
    public async Task UserRedirect_WhenTrustOrLocalAuthorityUser_RedirectsToYourSchools(
        string categoryId, string? uid, string? establishmentNumber)
    {
        Fixture.TestUserOrganisation.Set(Organisation("Test Organisation", categoryId, uid, establishmentNumber));

        var response = await Fixture.NonRedirectingClient.GetAsync("/user/redirect");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.OriginalString.Should().Be(Routes.YourSchools);
    }

    private static string[] SchoolNames(AngleSharp.Dom.IDocument page) =>
        page.QuerySelectorAll("#listView .app-school-result > a")
            .Select(a => a.TextContent.Trim())
            .ToArray();

    private static Organisation Organisation(string name, string categoryId, string? uid = null, string? establishmentNumber = null) =>
        new()
        {
            Id = "test-org",
            Name = name,
            Category = new Category { Id = categoryId },
            Uid = uid,
            EstablishmentNumber = establishmentNumber
        };
}
