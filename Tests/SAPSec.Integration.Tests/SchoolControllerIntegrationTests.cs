using System.Net;
using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;

namespace SAPSec.Integration.Tests;

[Collection("IntegrationTestsCollection")]
public class SchoolControllerIntegrationTests(WebApplicationSetupFixture fixture)
{
    private const string SchoolOverviewPath = "/school/105574";
    private const string SchoolDetailsPath = "/school/105574/school-details";

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
}
