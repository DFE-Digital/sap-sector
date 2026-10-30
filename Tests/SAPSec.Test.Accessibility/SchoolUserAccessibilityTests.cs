using Deque.AxeCore.Playwright;
using FluentAssertions;
using SAPSec.Core.Authentication;
using SAPSec.Test.Accessibility.Setup;
using SAPSec.Test.EndToEnd.Setup;
using SAPSec.Web.Constants;
using Xunit;

namespace SAPSec.Test.Accessibility;

/// <summary>
/// Accessibility of the pages a school user (organisation category 001) sees: their own school's pages,
/// and the Access denied page they get for anything else.
/// </summary>
[Collection("AccessibilityTestsCollection")]
public class SchoolUserAccessibilityTests(AccessibilityTestsFixture fixture) : AccessibilityTests(fixture)
{
    private const string PrimaryUrn = "101206";
    private const string SecondaryUrn = "100182";
    private const string AllThroughUrn = "100171";

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        Fixture.TestUser.CategoryId = OrganisationCategoryIds.Establishment;
    }

    public override async Task DisposeAsync()
    {
        Fixture.TestUser.Reset();
        await base.DisposeAsync();
    }

    public static TheoryData<string, string, string> OwnSchoolPages()
    {
        var data = new TheoryData<string, string, string>();

        var primary = Routes.PrimarySchool(PrimaryUrn);
        foreach (var path in new[]
                 {
                     primary.Overview, primary.KS2, primary.Attendance, primary.ViewSimilarSchools,
                     primary.SchoolDetails, primary.WhatIsASimilarSchool, primary.RiseResources,
                     primary.Comparison("101230").Similarity, primary.Comparison("101230").Ks2,
                     primary.Comparison("101230").Attendance, primary.Comparison("101230").SchoolDetails
                 })
        {
            data.Add(PrimaryUrn, "Primary", path);
        }

        var secondary = Routes.SecondarySchool(SecondaryUrn);
        foreach (var path in new[]
                 {
                     secondary.Overview, secondary.KS4HeadlineMeasures, secondary.KS4CoreSubjects, secondary.Attendance,
                     secondary.ViewSimilarSchools, secondary.SchoolDetails, secondary.WhatIsASimilarSchool,
                     secondary.Comparison("136555").Similarity, secondary.Comparison("136555").KS4HeadlineMeasures,
                     secondary.Comparison("136555").KS4CoreSubjects, secondary.Comparison("136555").Attendance,
                     secondary.Comparison("136555").SchoolDetails
                 })
        {
            data.Add(SecondaryUrn, "Secondary", path);
        }

        var allThrough = Routes.AllThroughSchool(AllThroughUrn);
        foreach (var path in new[]
                 {
                     allThrough.Overview, allThrough.KS2, allThrough.KS4HeadlineMeasures, allThrough.KS4CoreSubjects,
                     allThrough.Attendance, allThrough.ViewSimilarSchools, allThrough.SchoolDetails,
                     allThrough.WhatIsASimilarSchool
                 })
        {
            data.Add(AllThroughUrn, "All-through", path);
        }

        return data;
    }

    public static TheoryData<string> DeniedPages() =>
    [
        Routes.FindASchool(),
        Routes.FindASchool("School"),
        Routes.SecondarySchool("108088").Overview,
        Routes.SecondarySchool("108088").ViewSimilarSchools,
        Routes.PrimarySchool("999999").Overview,
        Routes.AllThroughSchool("999999").ViewSimilarSchools
    ];

    [Theory]
    [MemberData(nameof(OwnSchoolPages))]
    public async Task OwnSchoolPages_PassAxeAccessibilityChecks(string urn, string phase, string path)
    {
        SignInAsSchool(urn, phase);
        await NavigateTo(path);

        var results = await Page.RunAxe();

        var seriousViolations = results.Violations
            .Where(v => v.Impact == "critical" || v.Impact == "serious")
            .ToList();

        seriousViolations.Should().BeEmpty(
            $"Found violations: {string.Join(", ", seriousViolations.Select(v => v.Description))}");
    }

    [Theory]
    [MemberData(nameof(OwnSchoolPages))]
    public async Task OwnSchoolPages_DoNotOfferFindASchool(string urn, string phase, string path)
    {
        SignInAsSchool(urn, phase);
        await NavigateTo(path);

        var findASchoolLinks = await Page.Locator(".govuk-service-navigation").GetByText("Find a school").CountAsync();

        findASchoolLinks.Should().Be(0, "school users must not see the Find a school navigation item");
    }

    [Theory]
    [MemberData(nameof(OwnSchoolPages))]
    public async Task OwnSchoolPages_DoNotShowBreadcrumbs(string urn, string phase, string path)
    {
        SignInAsSchool(urn, phase);
        await NavigateTo(path);

        var breadcrumbs = await Page.Locator(".govuk-breadcrumbs").CountAsync();

        breadcrumbs.Should().Be(0, "school users must not see the breadcrumb navigation");
    }

    [Theory]
    [InlineData("/cookies")]
    [InlineData("/terms-and-conditions")]
    public async Task StaticPages_AsSchoolUser_HaveNoBreadcrumbs_AndPassAxeAccessibilityChecks(string path)
    {
        SignInAsSchool(SecondaryUrn, "Secondary");
        await NavigateTo(path);

        (await Page.Locator(".govuk-breadcrumbs").CountAsync()).Should().Be(0);

        var results = await Page.RunAxe();
        var seriousViolations = results.Violations
            .Where(v => v.Impact == "critical" || v.Impact == "serious")
            .ToList();

        seriousViolations.Should().BeEmpty(
            $"Found violations: {string.Join(", ", seriousViolations.Select(v => v.Description))}");
    }

    [Theory]
    [MemberData(nameof(DeniedPages))]
    public async Task AccessDeniedPage_PassesAxeAccessibilityChecks(string path)
    {
        SignInAsSchool(SecondaryUrn, "Secondary");

        var response = await Page.GotoAsync(path);
        response.Should().NotBeNull();
        response!.Status.Should().Be(403);
        await Page.WaitForSelectorAsync("main");

        var results = await Page.RunAxe();

        var seriousViolations = results.Violations
            .Where(v => v.Impact == "critical" || v.Impact == "serious")
            .ToList();

        seriousViolations.Should().BeEmpty(
            $"Found violations: {string.Join(", ", seriousViolations.Select(v => v.Description))}");
    }

    private void SignInAsSchool(string urn, string phase)
    {
        Fixture.TestUser.Urn = urn;
        Fixture.TestUser.Phase = phase;
    }
}
