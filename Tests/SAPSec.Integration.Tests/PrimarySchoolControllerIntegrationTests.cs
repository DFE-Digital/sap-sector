using System.Net;
using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;

namespace SAPSec.Integration.Tests;

[Collection("IntegrationTestsCollection")]
public class PrimarySchoolControllerIntegrationTests(WebApplicationSetupFixture fixture)
{
    private const string PrimarySchoolOverviewPath = "/school/primary/105574";
    private const string PrimaryWhatIsASimilarSchoolPath = "/school/primary/105574/what-is-a-similar-school";

    [Fact]
    public async Task GetPrimarySchoolOverview_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync(PrimarySchoolOverviewPath);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetPrimarySchoolOverview_UsesCurrentSchoolUrnInSimilarSchoolLink()
    {
        var response = await fixture.Client.GetAsync(PrimarySchoolOverviewPath);
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("href=\"/school/primary/105574/what-is-a-similar-school\"");
    }

    [Fact]
    public async Task GetPrimarySchoolOverview_ContainsPrimaryNavigation()
    {
        var response = await fixture.Client.GetAsync(PrimarySchoolOverviewPath);
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("Overview");
        content.Should().Contain("What is a similar school?");
    }

    [Fact]
    public async Task GetPrimaryWhatIsASimilarSchool_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync(PrimaryWhatIsASimilarSchoolPath);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetPrimaryWhatIsASimilarSchool_DoesNotContainBrokenSimilarSchoolsLink()
    {
        var response = await fixture.Client.GetAsync(PrimaryWhatIsASimilarSchoolPath);
        var content = await response.Content.ReadAsStringAsync();

        content.Should().NotContain("href=\"\"");
        content.Should().Contain("You will be able to view the full list of schools most similar to this one");
    }
}
