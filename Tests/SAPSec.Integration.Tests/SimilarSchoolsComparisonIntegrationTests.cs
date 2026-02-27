using System.Net;
using FluentAssertions;
using SAPSec.Integration.Tests.Infrastructure;

namespace SAPSec.Integration.Tests;

[Collection("IntegrationTestsCollection")]
public class SimilarSchoolsComparisonIntegrationTests(WebApplicationSetupFixture fixture)
{
    private const string ComparisonBasePath = "/school/108088/view-similar-schools/108075";

    [Fact]
    public async Task GetSimilarity_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync(ComparisonBasePath);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetSimilarity_ContainsComparisonHeadingAndTable()
    {
        var response = await fixture.Client.GetAsync(ComparisonBasePath);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("How these schools compare");
        content.Should().Contain("govuk-table");
        content.Should().Contain("Characteristic");
    }

    [Fact]
    public async Task GetSchoolDetails_ReturnsSuccess()
    {
        var response = await fixture.Client.GetAsync($"{ComparisonBasePath}/SchoolDetails");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetSchoolDetails_ContainsExpectedSections()
    {
        var response = await fixture.Client.GetAsync($"{ComparisonBasePath}/SchoolDetails");
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("School Details");
        content.Should().Contain("Compare sections");
    }
}
