using FluentAssertions;
using SAPSec.Test.Integration.Setup;
using SAPSec.Web.Constants;
using System.Net;
using System.Text.RegularExpressions;

namespace SAPSec.Test.Integration.Tests;

[Collection("JsonRepositoryIntegrationTestsCollection")]
public class SimilarSchoolsIntegrationTests(JsonRepositoryIntegrationTestFixture fixture)
{
    private static readonly string SimilarSchoolsPath = Routes.SecondarySchool("105574").ViewSimilarSchools;
    private static readonly string MissingSimilarSchoolsPath = Routes.SecondarySchool("999999").ViewSimilarSchools;
    private static readonly string SimilarSchoolsRedirectPath = $"{Routes.SecondarySchool("105574").Overview}/similar-schools";
    private static readonly string ComparisonHeadlineMeasuresPath =
        Routes.SecondarySchool("108088").Comparison("137621").KS4HeadlineMeasures;

    [Fact]
    public async Task ViewSimilarSchools_SchoolNotFound_RedirectsToNotFound()
    {
        var response = await fixture.Client.GetAsync(MissingSimilarSchoolsPath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
        content.Should().Contain("Page not found");
    }

    [Fact]
    public async Task GetSimilarSchools_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync(SimilarSchoolsPath);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetSimilarSchools_ContainsFilterForm()
    {
        var response = await fixture.Client.GetAsync(SimilarSchoolsPath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("app-filter-panel");
    }

    [Fact]
    public async Task GetSimilarSchools_ContainsResultsList()
    {
        var response = await fixture.Client.GetAsync(SimilarSchoolsPath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("app-school-results");
    }

    [Fact]
    public async Task GetSimilarSchools_ContainsToggleLink()
    {
        var response = await fixture.Client.GetAsync(SimilarSchoolsPath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("toggleViewLink");
    }

    [Fact]
    public async Task GetSimilarSchools_NoResults_HidesMapToggleAndShowsNoResultsMessage()
    {
        var response = await fixture.Client.GetAsync($"{SimilarSchoolsPath}?ur=doesnotexist");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("There are no schools that match your search.");
        content.Should().NotContain("toggleViewLink");
        content.Should().NotContain("View on map");
    }

    [Fact]
    public async Task GetSimilarSchools_RouteRedirectsToViewSimilarSchools()
    {
        var response = await fixture.NonRedirectingClient.GetAsync(SimilarSchoolsRedirectPath);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.MovedPermanently, HttpStatusCode.RedirectKeepVerb);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain(SimilarSchoolsPath);
    }

    [Fact]
    public async Task ComparisonKs4HeadlineMeasures_ReturnsComparisonContent()
    {
        var response = await fixture.Client.GetAsync(ComparisonHeadlineMeasuresPath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
        content.Should().Contain("KS4 headline performance measures");
        content.Should().Contain("Progress 8");
        content.Should().Contain("Attainment 8");
    }

    [Fact]
    public async Task ComparisonKs4HeadlineMeasures_BarChartsUseExpectedDecimalPlaces()
    {
        var response = await fixture.Client.GetAsync(ComparisonHeadlineMeasuresPath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        GetCanvasMarkup(content, "ks4-attainment8-comparison-chart").Should().Contain("data-label-decimals=\"1\"");
        GetCanvasMarkup(content, "eng-maths-comparison-chart").Should().Contain("data-label-decimals=\"0\"");
        GetCanvasMarkup(content, "destinations-comparison-chart").Should().Contain("data-label-decimals=\"0\"");
    }

    [Fact]
    public async Task ComparisonKs4HeadlineMeasures_Attainment8YearByYear_DisablesAxisAutoSkip()
    {
        var response = await fixture.Client.GetAsync(ComparisonHeadlineMeasuresPath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        GetCanvasMarkup(content, "ks4-attainment8-comparison-yearbyyear-chart").Should().Contain("data-axis-auto-skip=\"false\"");
    }

    private static string GetCanvasMarkup(string content, string id)
    {
        var pattern = $"""<canvas[^>]*id="{Regex.Escape(id)}"[^>]*>""";
        var match = Regex.Match(content, pattern, RegexOptions.Singleline);

        match.Success.Should().BeTrue($"expected canvas '{id}' to be rendered");
        return match.Value;
    }
}
