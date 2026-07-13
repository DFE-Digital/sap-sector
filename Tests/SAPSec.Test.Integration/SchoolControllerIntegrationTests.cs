using System.Net;
using System.Text.Json;
using AngleSharp.Dom;
using FluentAssertions;
using SAPSec.Test.Integration.Setup;

namespace SAPSec.Integration.Tests;

[Collection("IntegrationTestsCollection")]
public class SchoolControllerIntegrationTests(IntegrationTestFixture fixture)
{
    private const string SchoolOverviewPath = "/school/105574";
    private const string SchoolAttendancePath = "/school/105574/attendance";
    private const string SchoolDetailsPath = "/school/105574/school-details";
    private const string WhatIsASimilarSchoolPath = "/school/105574/what-is-a-similar-school";

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
    public async Task GetSchoolAttendance_ContainsUpdatedInsetContentAndLinks()
    {
        var response = await fixture.Client.GetAsync(SchoolAttendancePath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Compare this school's attendance measures with:");
        content.Should().Contain("<li>the local authority average</li>");
        content.Should().Contain("<li>the national average</li>");
        content.Should().NotContain("similar secondary phase schools");
        content.Should().Contain("To compare this school with similar schools using up-to-date attendance data, use the Monitor your school attendance service.");
        content.Should().Contain("href=\"https://viewyourdata.education.gov.uk/\"");
        content.Should().Contain(">View your education data (VYED) (opens in new tab)</a>");
        content.Should().Contain("href=\"https://viewyourdata.education.gov.uk/Account/Help\"");
        content.Should().Contain(">get help on accessing VYED (opens in new tab)</a>");
        content.Should().Contain("target=\"_blank\" rel=\"noopener noreferrer\"");
    }

    [Fact]
    public async Task GetSchoolAttendance_OffersBothAbsenceTypes_AndDoesNotRenderTopPerformersTab()
    {
        var document = await fixture.RequestPageAsync(SchoolAttendancePath);

        document.QuerySelector("h1")?.TextContent.Trim().Should().Be("Attendance measures");

        var absenceOptions = document.QuerySelectorAll("#attendanceAbsenceType option")
            .Select(option => (
                Value: option.GetAttribute("value"),
                Text: option.TextContent.Trim()))
            .ToArray();

        absenceOptions.Should().BeEquivalentTo(
            [("overall", "Overall absence"), ("persistent", "Persistent absence")],
            options => options.WithStrictOrdering());

        var tabTexts = document.QuerySelectorAll(".app-attendance-tabs .govuk-tabs__tab")
            .Select(tab => tab.TextContent.Trim())
            .ToArray();

        tabTexts.Should().NotContain("Top performers");
        tabTexts.Should().Contain("Year by year");
        tabTexts.Should().Contain("Table");
    }

    [Fact]
    public async Task GetSchoolAttendanceData_PersistentAbsence_ReturnsPersistentDataset()
    {
        var response = await fixture.Client.GetAsync("/school/105574/attendance-data?absenceType=persistent");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var json = JsonDocument.Parse(content);
        var root = json.RootElement;

        root.GetProperty("absenceType").GetString().Should().Be("persistent");
        root.GetProperty("bar").GetArrayLength().Should().Be(3);
        root.GetProperty("line").GetProperty("school").GetArrayLength().Should().Be(3);
        root.GetProperty("table").GetProperty("school").GetArrayLength().Should().Be(3);
        root.GetProperty("topPerformers").ValueKind.Should().Be(JsonValueKind.Array);
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
