using System.Net;
using System.Text.Json;
using AngleSharp.Dom;
using FluentAssertions;
using SAPSec.Test.Common.AngleSharp;
using SAPSec.Test.Integration.Setup;

namespace SAPSec.Test.Integration.Tests;

[Collection("JsonRepositoryIntegrationTestsCollection")]
public class SchoolControllerIntegrationTests(JsonRepositoryIntegrationTestFixture fixture)
{
    private const string SchoolOverviewPath = "/school/secondary/105574";
    private const string SchoolAttendancePath = "/school/secondary/105574/attendance";
    private const string SchoolDetailsPath = "/school/secondary/105574/school-details";
    private const string WhatIsASimilarSchoolPath = "/school/secondary/105574/what-is-a-similar-school";

    [Fact]
    public async Task GetSchoolOverview_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync(SchoolOverviewPath);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetSchoolOverview_ContainsExpectedContent()
    {
        var response = await fixture.Client.GetAsync(SchoolOverviewPath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Compare school performance");
        content.Should().Contain("View similar schools");
        content.Should().Contain("What is a similar school?");
    }

    [Fact]
    public async Task GetSchoolOverview_ContainsSecondaryNavigationInExpectedOrder()
    {
        var response = await fixture.Client.GetAsync(SchoolOverviewPath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        AssertInOrder(content,
            "Overview",
            "KS4 headline measures",
            "KS4 core subjects",
            "Attendance",
            "View similar schools",
            "School details",
            "What is a similar school?");
    }

    [Fact]
    public async Task GetSchoolOverview_HomeBreadcrumb_LinksToSchoolSearch()
    {
        var response = await fixture.Client.GetAsync(SchoolOverviewPath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("href=\"/find-a-school\">Home</a>");
    }

    [Fact]
    public async Task GetSchoolDetails_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync(SchoolDetailsPath);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetSchoolDetails_ContainsExpectedSections()
    {
        var response = await fixture.Client.GetAsync(SchoolDetailsPath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("School details");
        content.Should().Contain("Location");
        content.Should().Contain("Contact details");
        content.Should().Contain("Further information");
    }

    [Fact]
    public async Task GetSchoolAttendance_OffersBothAbsenceTypes()
    {
        var document = await fixture.RequestPageAsync(SchoolAttendancePath);

        var absenceTypeSelect = document.ElementWithTestIdShouldExist("attendance-absence-type");
        var absenceOptions = absenceTypeSelect.ChildTrimmedTextContent().ToArray();

        absenceOptions.Should().BeEquivalentTo(
            ["Overall absence", "Persistent absence"],
            options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetSchoolAttendance_DoesNotRenderTopPerformersTab()
    {
        var document = await fixture.RequestPageAsync(SchoolAttendancePath);

        var attendanceTabs = document.ElementWithTestIdShouldExist("attendance-tabs");
        var tabTexts = attendanceTabs.ChildTrimmedTextContent().ToArray();

        tabTexts.Should().NotContain("Top performers");
        tabTexts.Should().Contain("Year by year");
        tabTexts.Should().Contain("Table");
    }

    [Fact]
    public async Task GetWhatIsASimilarSchool_UsesGovUkLinkStylingForReferenceLinks()
    {
        var response = await fixture.Client.GetAsync(WhatIsASimilarSchoolPath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("href='https://www.gov.uk/government/statistics/english-indices-of-deprivation-2025/english-indices-of-deprivation-2025-statistical-release'");
        content.Should().Contain("href='https://www.officeforstudents.org.uk/data-and-analysis/young-participation-by-area/about-polar-and-adult-he/'");
        content.Should().Contain("class=\"govuk-link\" target=\"_blank\" rel=\"noopener noreferrer\"");
        content.Should().Contain("opens in new tab");
    }

    private static void AssertInOrder(string content, params string[] expectedText)
    {
        var currentIndex = -1;

        foreach (var text in expectedText)
        {
            var nextIndex = content.IndexOf(text, currentIndex + 1, StringComparison.Ordinal);
            nextIndex.Should().BeGreaterThan(currentIndex, $"expected '{text}' to appear after the previous navigation item");
            currentIndex = nextIndex;
        }
    }
}
