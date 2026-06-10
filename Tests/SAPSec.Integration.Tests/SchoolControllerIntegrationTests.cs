using System.Net;
using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;

namespace SAPSec.Integration.Tests;

[Collection("IntegrationTestsCollection")]
public class SchoolControllerIntegrationTests(WebApplicationSetupFixture fixture)
{
    private const string SchoolOverviewPath = "/school/105574";
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
