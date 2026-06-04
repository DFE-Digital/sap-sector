using System.Net;
using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;

namespace SAPSec.Integration.Tests;

[Collection("IntegrationTestsCollection")]
public class PrimarySchoolControllerIntegrationTests(WebApplicationSetupFixture fixture)
{
    private const string PrimarySchoolOverviewPath = "/school/primary/105574";
    private const string PrimarySchoolKs2Path = "/school/primary/105574/ks2";
    private const string PrimarySchoolAttendancePath = "/school/primary/105574/attendance";
    private const string PrimarySchoolViewSimilarSchoolsPath = "/school/primary/105574/view-similar-schools";
    private const string PrimarySchoolDetailsPath = "/school/primary/105574/school-details";
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

        AssertInOrder(content,
            "Overview",
            "KS2",
            "Attendance",
            "View similar schools",
            "School details",
            "What is a similar school?");
        content.Should().Contain("Show navigation");
    }

    [Theory]
    [InlineData(PrimarySchoolOverviewPath)]
    [InlineData(PrimarySchoolKs2Path)]
    [InlineData(PrimarySchoolAttendancePath)]
    [InlineData(PrimarySchoolViewSimilarSchoolsPath)]
    [InlineData(PrimarySchoolDetailsPath)]
    [InlineData(PrimaryWhatIsASimilarSchoolPath)]
    public async Task PrimaryNavigationPages_ReturnSuccess(string path)
    {
        var response = await fixture.Client.GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(PrimarySchoolOverviewPath, "Overview")]
    [InlineData(PrimarySchoolKs2Path, "KS2")]
    [InlineData(PrimarySchoolAttendancePath, "Attendance")]
    [InlineData(PrimarySchoolViewSimilarSchoolsPath, "View similar schools")]
    [InlineData(PrimarySchoolDetailsPath, "School details")]
    [InlineData(PrimaryWhatIsASimilarSchoolPath, "What is a similar school?")]
    public async Task PrimaryNavigation_ShowsSelectedTabAsActive(string path, string selectedTabText)
    {
        var response = await fixture.Client.GetAsync(path);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain(selectedTabText);
        content.Should().Contain("app-side-navigation__link--selected");
        content.Should().Contain("aria-current=\"page\"");
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
        content.Should().Contain("href=\"/school/primary/105574/view-similar-schools\"");
        content.Should().Contain("view all the schools most similar to this one");
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
