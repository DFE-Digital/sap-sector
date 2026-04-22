using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;
using System.Net;

namespace SAPSec.Integration.Tests;

[Collection("IntegrationTestsCollection")]
public class SimilarSchoolsIntegrationTests(WebApplicationSetupFixture fixture)
{
    [Fact]
    public async Task ViewSimilarSchools_SchoolNotFound_RedirectsToNotFound()
    {
        var response = await fixture.Client.GetAsync("/school/999999/view-similar-schools");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
        content.Should().Contain("Page not found");
    }

    [Fact]
    public async Task GetSimilarSchools_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/school/105574/view-similar-schools");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetSimilarSchools_ContainsFilterForm()
    {
        var response = await fixture.Client.GetAsync("/school/105574/view-similar-schools");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("app-filter-panel");
    }

    [Fact]
    public async Task GetSimilarSchools_ContainsResultsList()
    {
        var response = await fixture.Client.GetAsync("/school/105574/view-similar-schools");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("app-school-results");
    }

    [Fact]
    public async Task GetSimilarSchools_ContainsToggleLink()
    {
        var response = await fixture.Client.GetAsync("/school/105574/view-similar-schools");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("toggleViewLink");
    }

    [Fact]
    public async Task GetSimilarSchools_RouteRedirectsToViewSimilarSchools()
    {
        var response = await fixture.NonRedirectingClient.GetAsync("/school/105574/similar-schools");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.MovedPermanently, HttpStatusCode.RedirectKeepVerb);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/school/105574/view-similar-schools");
    }

    [Fact]
    public async Task ComparisonKs4HeadlineMeasures_ReturnsComparisonContent()
    {
        var response = await fixture.Client.GetAsync("/school/108088/view-similar-schools/137621/Ks4HeadlineMeasures");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
        content.Should().Contain("KS4 headline performance measures");
        content.Should().Contain("Progress 8");
        content.Should().Contain("Attainment 8");
    }
}
