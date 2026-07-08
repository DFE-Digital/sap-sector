using FluentAssertions;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;

namespace SAPSec.Test.Integration;

[Collection("JsonRepositoryIntegrationTestsCollection")]
public class PrimarySchoolControllerIntegrationTests(JsonRepositoryIntegrationTestFixture fixture)
{
    private const string PrimarySchoolUrn = "100134";

    private static readonly PageTestCase[] AllPagePaths = [
        new(Routes.PrimarySchool(PrimarySchoolUrn).Overview, "Overview"),
        new(Routes.PrimarySchool(PrimarySchoolUrn).KS2, "KS2"),
        new(Routes.PrimarySchool(PrimarySchoolUrn).Attendance, "Attendance"),
        new(Routes.PrimarySchool(PrimarySchoolUrn).ViewSimilarSchools, "View similar schools"),
        new(Routes.PrimarySchool(PrimarySchoolUrn).SimilarSchoolComparison("100134"), null),
        new(Routes.PrimarySchool(PrimarySchoolUrn).SchoolDetails, "School details"),
        new(Routes.PrimarySchool(PrimarySchoolUrn).WhatIsASimilarSchool, "What is a similar school?")
    ];

    [Fact]
    public async Task GetPrimarySchoolOverview_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync(Routes.PrimarySchool(PrimarySchoolUrn).Overview);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetPrimarySchoolOverview_UsesCurrentSchoolUrnInSimilarSchoolLink()
    {
        var response = await fixture.Client.GetAsync(Routes.PrimarySchool(PrimarySchoolUrn).Overview);
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain($"href=\"/school/primary/{PrimarySchoolUrn}/what-is-a-similar-school\"");
    }

    [Fact]
    public async Task GetPrimarySchoolOverview_ContainsPrimaryNavigation()
    {
        var response = await fixture.Client.GetAsync(Routes.PrimarySchool(PrimarySchoolUrn).Overview);
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
    [MemberData(nameof(AllPages))]
    public async Task PrimaryNavigationPages_ReturnSuccess(string path)
    {
        var response = await fixture.Client.GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [MemberData(nameof(AllPagesWithSelectedTabText))]
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
        var response = await fixture.Client.GetAsync(Routes.PrimarySchool(PrimarySchoolUrn).WhatIsASimilarSchool);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetPrimaryWhatIsASimilarSchool_DoesNotContainBrokenSimilarSchoolsLink()
    {
        var response = await fixture.Client.GetAsync(Routes.PrimarySchool(PrimarySchoolUrn).WhatIsASimilarSchool);
        var content = await response.Content.ReadAsStringAsync();

        content.Should().NotContain("href=\"\"");
        content.Should().Contain($"href=\"/school/primary/{PrimarySchoolUrn}/view-similar-schools\"");
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

    public static TheoryData<string> AllPages()
    {
        var data = new TheoryData<string>();
        foreach (var (path, _) in AllPagePaths)
        {
            data.Add(path);
        }

        return data;
    }

    public static TheoryData<string, string> AllPagesWithSelectedTabText()
    {
        var data = new TheoryData<string, string>();
        foreach (var (path, selectedTabText) in AllPagePaths)
        {
            if (selectedTabText is not null)
            {
                data.Add(path, selectedTabText);
            }
        }

        return data;
    }

    private record PageTestCase(string Path, string? SelectedTabText);
}
