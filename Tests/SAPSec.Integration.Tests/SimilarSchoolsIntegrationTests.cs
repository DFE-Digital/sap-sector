using System.Net;
using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;

namespace SAPSec.Integration.Tests;

[Collection("IntegrationTestsCollection")]
public class SimilarSchoolsIntegrationTests(WebApplicationSetupFixture fixture)
{
    [Fact]
    public async Task GetSimilarSchools_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync("/school/147788/view-similar-schools");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetSimilarSchools_ContainsFilterForm()
    {
        var response = await fixture.Client.GetAsync("/school/147788/view-similar-schools");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("similar-schools-filter-form");
    }

    [Fact]
    public async Task GetSimilarSchools_ContainsResultsList()
    {
        var response = await fixture.Client.GetAsync("/school/147788/view-similar-schools");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("app-school-results");
    }

    [Fact]
    public async Task GetSimilarSchools_ContainsToggleLink()
    {
        var response = await fixture.Client.GetAsync("/school/147788/view-similar-schools");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("toggleViewLink");
    }

    [Fact]
    public async Task GetSimilarSchools_RouteRedirectsToViewSimilarSchools()
    {
        var response = await fixture.NonRedirectingClient.GetAsync("/school/147788/similar-schools");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.MovedPermanently, HttpStatusCode.RedirectKeepVerb);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/school/147788/view-similar-schools");
    }
}
